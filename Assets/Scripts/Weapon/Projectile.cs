using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 startPos;
    private float range;
    private float damage; // 데미지 추가

    // Setup 함수에 damage 인자 추가
    public void Setup(float speed, float range, float damage, Vector2 direction)
    {
        this.range = range;
        this.damage = damage;
        this.startPos = transform.position;

        // 투사체 이동 설정
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;

        // 투사체가 날아가는 방향을 바라보게 회전 (필요 시)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        // 사거리 체크
        float distanceTraveled = Vector2.Distance(startPos, transform.position);
        if (distanceTraveled >= range)
        {
            Destroy(gameObject);
        }
    }

    // 적이나 벽에 충돌했을 때 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 적에게 데미지 입히기 (태그가 Enemy인 경우)
        if (collision.CompareTag("Enemy"))
        {
            // 적의 체력 관리 스크립트를 가져옵니다.
            var enemy = collision.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                // 계산되어 넘어온 80% 데미지를 입힙니다.
                enemy.TakeDamage(damage);
                Debug.Log($"적중! 데미지: {damage} (플레이어 공격력의 80%)");
            }
            Destroy(gameObject);
        }

        // 2. 벽에 부딪혔을 때
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject); // 벽에 닿으면 소멸
        }
    }
}