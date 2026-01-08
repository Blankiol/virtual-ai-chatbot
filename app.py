from flask import Flask, render_template, request, jsonify
from openai import OpenAI

app = Flask(__name__)

# [Ollama 설정]
client = OpenAI(
    base_url="http://localhost:11434/v1",
    api_key="ollama"
)


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
# [중요] 서버 실행 코드는 파일의 '맨 마지막'에 두는 것이 정석입니다.
# ======================================================
if __name__ == '__main__':
    # 0.0.0.0으로 열면 외부(Unity, 핸드폰)에서 접속 가능합니다.
    app.run(host='0.0.0.0', port=5000, debug=True) #debug=True는 개발용입니다. 배포 시 False로 변경하세요.