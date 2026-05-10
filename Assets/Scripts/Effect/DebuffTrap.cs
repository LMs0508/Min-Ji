using UnityEngine;

public class DebuffTrap : MonoBehaviour
{
    [Header("함정 설정")]
    public DebuffType debuffType;      // 어떤 디버프를 걸지 선택
    public float debuffPower = 0.5f;   // 디버프 위력 (슬로우 배율, 화상 데미지 등)
    public float debuffDuration = 3f;  // 디버프 지속 시간
    
    [Header("함정 옵션")]
    public bool destroyOnTrigger = true; // 밟으면 함정이 사라질지 여부
    public GameObject hitEffectPrefab;   // 함정 발동 시 터지는 이펙트 (선택 사항)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 충돌한 대상이 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            // 2. 플레이어(또는 자식)에게서 DebuffManager를 가져옴
            PlayerDebuffManager debuffManager = other.GetComponent<PlayerDebuffManager>();
            if (debuffManager == null) debuffManager = other.GetComponentInChildren<PlayerDebuffManager>();

            if (debuffManager != null)
            {
                // 3. 인스펙터에서 설정한 디버프 적용
                debuffManager.ApplyDebuff(debuffType, debuffPower, debuffDuration);
                
                Debug.Log($"<color=red>[함정 발동]</color> {debuffType} 디버프 적용! (위력: {debuffPower}, 시간: {debuffDuration}s)");

                // 4. 시각 효과 (이펙트 프리팹이 있다면 생성)
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }

                // 5. 일회용 함정 처리
                if (destroyOnTrigger)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}