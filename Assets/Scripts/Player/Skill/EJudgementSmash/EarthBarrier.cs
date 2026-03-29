using UnityEngine;
using System.Collections.Generic;

public class EarthBarrier : MonoBehaviour
{
    [Header("위치 설정")]
    public Vector3 positionOffset = new Vector3(0, 0.5f, 0);

    [Header("데미지 설정")]
    public int damage = 5;
    public float damageInterval = 0.1f;
    
    private Dictionary<Collider2D, float> damageTimers = new Dictionary<Collider2D, float>();
    private Collider2D myCollider;
    private ContactFilter2D filter;
    private List<Collider2D> overlapResults = new List<Collider2D>();

    private void Start()
    {
        transform.localPosition = positionOffset;
        myCollider = GetComponent<Collider2D>();
        filter = ContactFilter2D.noFilter;
        filter.useTriggers = true; // [핵심] 트리거 콜라이더인 보스의 다리도 감지하도록 강제 설정
    }

    private void LateUpdate()
    {
        transform.localPosition = positionOffset;
    }

    private void FixedUpdate()
    {
        if (myCollider == null) return;

        Physics2D.OverlapCollider(myCollider, filter, overlapResults);
        
        HashSet<Collider2D> currentColliders = new HashSet<Collider2D>();

        foreach (var col in overlapResults)
        {
            if (col.CompareTag("Enemy") && col.enabled && col.gameObject.activeInHierarchy)
            {
                currentColliders.Add(col);

                if (!damageTimers.ContainsKey(col))
                {
                    damageTimers[col] = damageInterval; // 닿자마자 즉시 데미지
                }

                damageTimers[col] += Time.fixedDeltaTime;

                if (damageTimers[col] >= damageInterval)
                {
                    EnemyHealth health = col.GetComponentInParent<EnemyHealth>();
                    if (health != null)
                    {
                        Vector2 pushDir = (col.transform.position - transform.position).normalized;
                        health.TakeDamage(damage, pushDir);
                    }
                    damageTimers[col] = 0f;
                }
            }
        }

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