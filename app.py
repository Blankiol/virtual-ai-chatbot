import os
import asyncio
import re
import edge_tts
from flask import Flask, render_template, request, jsonify, send_file
from openai import OpenAI

app = Flask(__name__)

# [Ollama 설정]
client = OpenAI(
    base_url="http://localhost:11434/v1",
    api_key="ollama"
)

# [설정] 목소리 타입 (한국어 여성: ko-KR-SunHiNeural 추천)
VOICE = "ko-KR-SunHiNeural" 
OUTPUT_FILE = "static/voice.mp3" # 저장될 파일 위치

# static 폴더가 없으면 만들기
if not os.path.exists('static'):
    os.makedirs('static')


@app.route('/')
def home():
    return render_template('index.html')

@app.route('/chat', methods=['POST'])
def chat():
    data = request.json
    user_message = data.get('message')
    character_setting = data.get('character_setting')

    if not user_message:
        return jsonify({'error': '메시지가 없습니다.'}), 400

    print(f"📩 Unity에서 받은 메시지: {user_message}")

    messages = [
        {
            "role": "system",
            "content": f"""
            당신은 다음 설정의 캐릭터를 연기해야 합니다:
            {character_setting}

            [필수 규칙]
            1. 답변의 맨 앞에는 반드시 감정 태그를 붙이세요. 예: [Happy], [Sad], [Angry], [Neutral].
            2. 이모티콘(😊, 😋, 🍕 등)은 절대 사용하지 마세요. 오직 텍스트로만 답하세요.
            3. 한국어 문법을 정확하게 지키고, 비문(말이 안 되는 문장)을 쓰지 마세요.
            4. 질문에 대해 명확하고 논리적인 답변을 하세요. 횡설수설하지 마세요.
            5. 설정된 말투를 끝까지 유지하세요.
            """
        },
        {"role": "user", "content": user_message}
    ]

    try:
        # 모델 이름 확인 필요 (터미널에서 설치한 이름과 같아야 함)
        response = client.chat.completions.create(
            model="gemma2:2b",
            messages=messages,
            temperature=0.7
        )
        
        ai_reply = response.choices[0].message.content
        print(f"🤖 AI 답변: {ai_reply}")
        return jsonify({'reply': ai_reply})

    except Exception as e:
        print(f"❌ 에러 발생: {e}")
        return jsonify({'error': str(e)}), 500


# ======================================================
# [TTS 기능]
# ======================================================
@app.route('/tts', methods=['POST'])
def tts_generate():
    data = request.json
    text = data.get('text')
    
    if not text:
        return jsonify({'error': '텍스트가 없습니다.'}), 400

    # 이모티콘이나 감정 태그([Happy])는 읽으면 이상하니까 제거하는 게 좋습니다.
    # (간단하게 구현하기 위해 여기서는 생략하지만, 추후 제거 로직 추가 추천)

    try:
        # 정규표현식: 대괄호[]와 그 안의 글자를 찾아서 삭제함
        clean_text = re.sub(r'\[.*?\]', '', text).strip()

        # 텍스트가 비어버리면(태그만 있었을 경우) 기본값 설정
        if not clean_text:
            clean_text = "..."
        
        print(f"🗣️ 읽을 텍스트: {clean_text}") # 확인용 로그

        # 비동기 함수 실행
        asyncio.run(generate_audio(clean_text))
        
        # 만들어진 파일을 Unity로 보냄
        return send_file(OUTPUT_FILE, mimetype="audio/mpeg")
    
    except Exception as e:
        print(f"❌ TTS 에러: {e}")
        return jsonify({'error': str(e)}), 500

# 실제로 음성 파일을 만드는 함수
async def generate_audio(text):
    communicate = edge_tts.Communicate(text, VOICE)
    await communicate.save(OUTPUT_FILE)

# ======================================================
# [중요] 서버 실행 코드는 파일의 '맨 마지막'에 두는 것이 정석입니다.
# ======================================================
if __name__ == '__main__':
    # 0.0.0.0으로 열면 외부(Unity, 핸드폰)에서 접속 가능합니다.
    app.run(host='0.0.0.0', port=5000, debug=True) #debug=True는 개발용입니다. 배포 시 False로 변경하세요.