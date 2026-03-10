using UnityEngine;
using System.Collections.Generic;
using Game.Player;

public class WeaponChargeDamage : MonoBehaviour
{
    private CircleCollider2D myCol;
    private LayerMask enemyLayer;
    private PlayerStats stats;
    private float damageMult;
    private float knockback;
    private GameObject owner;

    // [최적화] 비할당 물리 연산을 위한 정적 배열 및 히스토리
    private Collider2D[] hitResults = new Collider2D[10];
    private HashSet<GameObject> hitHistory = new HashSet<GameObject>();

    private void Awake()
    {
        myCol = GetComponent<CircleCollider2D>();
    }

    // 공격 시작 전 데이터 주입 (WeaponCharge에서 호출)
    public void Setup(GameObject owner, PlayerStats stats, float mult, float kb, LayerMask layer)
    {
        this.owner = owner;
        this.stats = stats;
        this.damageMult = mult;
        this.knockback = kb;
        this.enemyLayer = layer;

        if (myCol == null) myCol = GetComponent<CircleCollider2D>();
        hitHistory.Clear(); // 새로운 공격 시 타격 기록 초기화
    }

    // 매 프레임 판정 수행
    public void CheckHit()
    {
        if (myCol == null || stats == null) return;

        // 월드 좌표 기준의 중심점과 실제 반지름 계산
        // 수식: $ActualRadius = ColliderRadius \times LossyScale.x$
        Vector2 center = transform.TransformPoint(myCol.offset);
        float actualRadius = myCol.radius * Mathf.Abs(transform.lossyScale.x);

        // 비할당 원형 탐색 (메모리 할당 0)
        int count = Physics2D.OverlapCircleNonAlloc(center, actualRadius, hitResults, enemyLayer);

        for (int i = 0; i < count; i++)
        {
            GameObject enemy = hitResults[i].gameObject;
            if (hitHistory.Contains(enemy)) continue;

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                int finalDamage = Mathf.RoundToInt(stats.Attack.Value * damageMult);
                Vector2 dir = (enemy.transform.position - owner.transform.position).normalized;

                // 넉백 처리
                Rigidbody2D erb = enemy.GetComponentInChildren<Rigidbody2D>();
                if (erb && knockback > 0)
                {
                    erb.linearVelocity = Vector2.zero;
                    erb.AddForce(dir * knockback, ForceMode2D.Impulse);
                }

                health.TakeDamage(finalDamage, dir);
                hitHistory.Add(enemy);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (myCol == null) myCol = GetComponent<CircleCollider2D>();
        if (myCol == null) return;

        // 판정 코드와 토씨 하나 안 틀리고 똑같은 수식 사용
        Vector3 center = transform.TransformPoint(myCol.offset);
        float radius = myCol.radius * Mathf.Abs(transform.lossyScale.x);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, radius);

        // 중심점 표시
        Gizmos.DrawLine(center + Vector3.left * 0.1f, center + Vector3.right * 0.1f);
        Gizmos.DrawLine(center + Vector3.up * 0.1f, center + Vector3.down * 0.1f);
    }
}