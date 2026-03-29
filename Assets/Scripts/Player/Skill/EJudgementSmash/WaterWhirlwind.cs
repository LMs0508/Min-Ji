using UnityEngine;
using System.Collections.Generic;

public class WaterWhirlwind : MonoBehaviour
{
    public int damage = 3;
    public float damageInterval = 0.1f;
    public float pullForce = 2f;
    private Dictionary<Collider2D, float> damageTimers = new Dictionary<Collider2D, float>();
    private Collider2D myCollider;
    private ContactFilter2D filter;
    private List<Collider2D> overlapResults = new List<Collider2D>();

    private void Start()
    {
        myCollider = GetComponent<Collider2D>();
        filter.NoFilter(); // 모든 콜라이더 감지
        filter.useTriggers = true; // [핵심] 트리거 콜라이더인 보스의 다리도 감지하도록 강제 설정
    }

    private void FixedUpdate()
    {
        if (myCollider == null) return;

        // 유니티 물리 엔진의 OnTriggerEnter 버그를 완벽하게 피하기 위해, 매 프레임 직접 겹친 콜라이더를 찾습니다.
        Physics2D.OverlapCollider(myCollider, filter, overlapResults);
        
        HashSet<Collider2D> currentColliders = new HashSet<Collider2D>();

        foreach (var col in overlapResults)
        {
            if (col.CompareTag("Enemy") && col.enabled && col.gameObject.activeInHierarchy)
            {
                currentColliders.Add(col);

                // 처음 닿은 적이라면 '즉시' 데미지를 주도록 타이머를 한계치로 설정합니다.
                if (!damageTimers.ContainsKey(col))
                {
                    damageTimers[col] = damageInterval; 
                }

                // 1. 데미지 처리
                damageTimers[col] += Time.fixedDeltaTime;
                if (damageTimers[col] >= damageInterval)
                {
                    EnemyHealth health = col.GetComponentInParent<EnemyHealth>();
                    if (health != null) health.TakeDamage(damage);
                    
                    damageTimers[col] = 0f; // 타이머 초기화
                }

                // 2. 끌어당기는 효과
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 pullDir = (transform.position - col.transform.position).normalized;
                    rb.AddForce(pullDir * pullForce);
                }
            }
        }

        // 범위를 벗어났거나 파괴된 콜라이더 정리 (메모리 누수 방지)
        List<Collider2D> collidersToRemove = new List<Collider2D>();
        foreach (var key in damageTimers.Keys)
        {
            if (!currentColliders.Contains(key))
            {
                collidersToRemove.Add(key);
            }
        }

        foreach (var key in collidersToRemove)
        {
            damageTimers.Remove(key);
        }
    }
}