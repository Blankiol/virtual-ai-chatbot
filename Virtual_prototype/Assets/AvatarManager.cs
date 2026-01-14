using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public Animator animator; // 캐릭터의 애니메이터

    // [추가된 부분] 게임이 시작될 때 자동으로 연결하기
    void Start()
    {
        // 만약 빈칸으로 비워져 있다면, 내 몸(GameObject)에 붙은 Animator를 찾아서 넣어라!
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    // [추가된 부분] 키보드로 테스트하기 위함
    void Update()
    {
        // 키보드 H를 누르면 Happy 발동
        if (Input.GetKeyDown(KeyCode.H))
        {
            ChangeEmotion("[happy]");
        }

        // 키보드 I를 누르면 Idle(기본)로 강제 복귀 (필요시)
        if (Input.GetKeyDown(KeyCode.I))
        {
            // animator.SetTrigger("DoIdle"); // DoIdle 트리거가 있다면 사용
        }
    }

    // 감정 태그가 들어오면 실행되는 함수
    public void ChangeEmotion(string tag)
    {
        // 안전장치: 애니메이터가 없으면 에러 나지 말고 그냥 무시해라
        if (animator == null) return;

        // 태그를 소문자로 바꿔서 비교 (실수 방지)
        string cleanTag = tag.ToLower().Trim();

        if (cleanTag.Contains("[happy]"))
        {
            animator.SetTrigger("DoHappy");
        }
        else if (cleanTag.Contains("[sad]"))
        {
            animator.SetTrigger("DoSad"); // Animator에 DoSad 트리거도 만들어야 함
        }
        else
        {
            // 기본 상태로 돌아가거나 할 때 사용
            animator.SetTrigger("DoIdle"); 
        }
    }
}