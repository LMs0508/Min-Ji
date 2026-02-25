using UnityEngine;

public class FireTrailDamage : MonoBehaviour
{
    [Header("설정")]
    public float damage = 5f;        // 틱당 데미지
    public float lifetime = 1.5f;    // 불길이 바닥에 머무는 시간
    public float damageInterval = 0.5f; // 데미지 주기 (0.5초마다)

    private float nextDamageTime;

    private void Start()
    {
        // 생성된 후 lifetime초 후에 자동으로 사라짐
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 적 태그 확인
        if (other.CompareTag("Enemy"))
        {
            if (Time.time >= nextDamageTime)
            {
                // 1. 부딪힌 대상(other)으로부터 EnemyHealth 컴포넌트를 가져옵니다.
                EnemyHealth health = other.GetComponent<EnemyHealth>();

                // 2. 컴포넌트가 존재한다면 클래스 이름이 아닌 'health' 변수를 통해 함수를 호출합니다.
                if (health != null)
                {
                    health.TakeDamage(damage);
                }

                Debug.Log($"{other.name}에게 불길 데미지 {damage} 적용!");
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }
}