using UnityEngine;

public class BlindTrap : MonoBehaviour
{
    [Header("함정 설정")]
    public float blindDuration = 4f; // 실명 지속 시간
    public float blindPower = 0.9f;  // 실명 강도 (0.9면 아주 어두움)
    public bool destroyOnTrigger = true; // 밟으면 함정이 사라질지 여부

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 충돌한 대상이 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            // 2. 플레이어에게서 DebuffManager를 가져옴
            PlayerDebuffManager debuffManager = other.GetComponent<PlayerDebuffManager>();

            if (debuffManager != null)
            {
                // 3. 실명 디버프 적용
                debuffManager.ApplyDebuff(DebuffType.Blind, blindPower, blindDuration);
                Debug.Log("<color=yellow>[함정]</color> 플레이어가 함정을 밟아 실명에 걸렸습니다!");

                // 4. 일회용 함정이라면 오브젝트 삭제
                if (destroyOnTrigger)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}