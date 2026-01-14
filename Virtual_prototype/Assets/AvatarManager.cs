using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;       // 춤추는 기능
    public AudioSource audioSource; // [추가됨] 목소리 내는 스피커

    // 게임이 시작될 때 부품 자동 연결
    void Start()
    {
        // 1. 애니메이터가 비어있으면 내 몸에서 찾아서 넣기
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // 2. [추가됨] 오디오 소스가 비어있으면 내 몸에서 찾아서 넣기
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // ---------------------------------------------------------
    // 1. 감정 표현 (몸 동작)
    // ---------------------------------------------------------
    public void ChangeEmotion(string tag)
    {
        // 안전장치: 애니메이터가 없으면 에러 나지 말고 무시
        if (animator == null) return;

        // 태그를 소문자로 바꿔서 비교 (실수 방지)
        string cleanTag = tag.ToLower().Trim();

        if (cleanTag.Contains("[happy]"))
        {
            animator.SetTrigger("DoHappy");
        }
        else if (cleanTag.Contains("[sad]"))
        {
            animator.SetTrigger("DoSad");
        }
        else
        {
            // 태그가 없거나 모르는 태그일 때
            animator.SetTrigger("DoIdle");
        }
    }

    // ---------------------------------------------------------
    // 2. [추가됨] 목소리 재생 (입)
    // ---------------------------------------------------------
    public void PlayVoice(AudioClip clip)
    {
        // 스피커가 있고, 오디오 파일이 정상적이라면 재생
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip; // 클립 장전
            audioSource.Play();      // 발사 (재생)
        }
    }
}