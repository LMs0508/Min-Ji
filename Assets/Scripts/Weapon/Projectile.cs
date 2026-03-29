using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    private Vector2 startPos;
    private float range;
    private float damage;
    private bool isPiercing; // 관통 옵션
    
    private HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>(); // 중복 타격 방지 리스트

    [Header("이펙트 설정")]
    [Tooltip("적이나 벽에 맞았을 때 생성될 피격/폭발 애니메이션 프리팹")]
    public GameObject hitEffectPrefab;

    // Setup 함수에 isPiercing 선택적 인자 추가
    public void Setup(float speed, float range, float damage, Vector2 direction, bool isPiercing = false)
    {
        this.range = range;
        this.damage = damage;
        this.isPiercing = isPiercing;
        this.startPos = transform.position;
        
        damagedEnemies.Clear(); // 셋업 시 리스트 초기화

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
            SpawnHitEffect();
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
            
            // [핵심] 아직 데미지를 주지 않은 적일 때만 데미지를 줍니다!
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                damagedEnemies.Add(enemy); // 데미지를 준 적으로 기록
                enemy.TakeDamage(damage);
                Debug.Log($"투사체 적중! 데미지: {damage}");
                SpawnHitEffect(); // 데미지가 들어갈 때만 이펙트 터뜨리기
            }
            
            // 관통 속성이 아닐 때만 투사체 소멸
            if (!isPiercing) 
                Destroy(gameObject);
        }

        // 2. 벽에 부딪혔을 때
        if (collision.CompareTag("Wall"))
        {
            SpawnHitEffect();
            
            // [핵심] 관통 옵션(레이저 등)일 때는 벽에 스쳐도 사라지지 않고 뚫고 지나갑니다!
            // 시원한 연출과 제대로 된 사거리를 보장하기 위한 조치입니다.
            if (!isPiercing) 
                Destroy(gameObject);
        }
    }

    // [추가] 투사체가 소멸될 때 이펙트 프리팹을 생성하는 함수
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, transform.rotation);
            Destroy(effect, 0.5f); // 피격 애니메이션 길이(예: 0.5초)만큼 보여준 뒤 자동 삭제
        }
    }
}