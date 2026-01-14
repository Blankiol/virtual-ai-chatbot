using UnityEngine;
using UnityEngine.Networking; // 통신용
using TMPro; // UI용
using System.Collections;

public class ChatManager : MonoBehaviour
{
    [Header("UI 연결 (드래그해서 넣으세요)")]
    public TMP_InputField inputField;  // 유저 입력창
    public TMP_Text chatDisplay;       // AI 답변 텍스트

    [Header("캐릭터 연결 (드래그해서 넣으세요)")]
    public AvatarManager avatarManager; // Part 2에서 만든 캐릭터 관리자

    // 파이썬 서버 주소
    private string chatUrl = "http://localhost:5000/chat";
    private string ttsUrl = "http://localhost:5000/tts";

    void Start()
    {
        // 시작하면 입력창에 포커스 두고, 엔터 치면 전송되게 설정
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnSubmit);
            inputField.ActivateInputField();
        }
    }

    // 엔터 쳤을 때 실행되는 함수
    void OnSubmit(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            // 1. 화면 비우고
            inputField.text = "";

            // 2. 서버로 질문 발사!
            StartCoroutine(ProcessChat(text));

            // 3. 다시 입력할 수 있게 포커스 유지
            inputField.ActivateInputField();
        }
    }

    // 통합 처리 과정 (채팅 -> 행동 -> 목소리)
    IEnumerator ProcessChat(string message)
    {
        // ====================================================
        // STEP 1: 뇌(LLM)에게 텍스트 보내고 답변 받기
        // ====================================================

        // JSON 만들기 (설정은 여기서 수정 가능)
        string json = "{\"message\":\"" + message + "\", \"character_setting\":\"너는 친절한 여동생 캐릭터야.\"}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(chatUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest(); // 기다림...

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("채팅 에러: " + request.error);
                yield break; // 에러나면 여기서 중단
            }

            // 답변 도착!
            string responseJson = request.downloadHandler.text;
            ChatResponse responseData = JsonUtility.FromJson<ChatResponse>(responseJson);
            string rawReply = responseData.reply; // 예: "[Happy] 반가워!"

            // ------------------------------------------------
            // 태그([Happy])와 대사("반가워!") 분리하기
            // ------------------------------------------------
            string emotionTag = "";
            string cleanMessage = rawReply;

            if (rawReply.StartsWith("["))
            {
                int closeIndex = rawReply.IndexOf("]");
                if (closeIndex != -1)
                {
                    emotionTag = rawReply.Substring(0, closeIndex + 1); // "[Happy]" 추출
                    cleanMessage = rawReply.Substring(closeIndex + 1).Trim(); // "반가워!" 추출
                }
            }

            // 화면에 글자 띄우기
            if (chatDisplay != null) chatDisplay.text = cleanMessage;

            // 캐릭터에게 감정 전달 (춤추기)
            if (avatarManager != null) avatarManager.ChangeEmotion(emotionTag);

            // ====================================================
            // STEP 2: 정제된 텍스트로 목소리(TTS) 요청하기
            // ====================================================
            StartCoroutine(GetVoice(cleanMessage));
        }
    }

    // 목소리 받아오기
    IEnumerator GetVoice(string textToSpeak)
    {
        string json = "{\"text\":\"" + textToSpeak + "\"}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(ttsUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(ttsUrl, AudioType.MPEG); // MP3 받기
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest(); // 기다림...

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 파일 받아서 오디오 클립으로 변환
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                // 캐릭터에게 말하라고 시키기
                if (avatarManager != null)
                {
                    avatarManager.PlayVoice(clip);
                }
            }
        }
    }
}

// JSON 해석용
[System.Serializable]
public class ChatResponse
{
    public string reply;
}