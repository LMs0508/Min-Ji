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

    // [추가] 코드에서 제어할 반지름 변수
    private float finalRadius;

    private Collider2D[] hitResults = new Collider2D[20]; // 배열 크기를 20으로 확장 (최적화)
    private HashSet<GameObject> hitHistory = new HashSet<GameObject>();

    private void Awake()
    {
        myCol = GetComponent<CircleCollider2D>();
    }

    // [수정] baseRadius를 인자로 받습니다.
    public void Setup(GameObject owner, PlayerStats stats, float mult, float kb, LayerMask layer, float radius)
    {
        this.owner = owner;
        this.stats = stats;
        this.damageMult = mult;
        this.knockback = kb;
        this.enemyLayer = layer;
        this.finalRadius = radius; // 전달받은 계산된 반지름 저장

        if (myCol == null) myCol = GetComponent<CircleCollider2D>();
        hitHistory.Clear();
    }

    public void CheckHit()
    {
        if (stats == null) return;

        // [최적화] 물리 엔진 동기화 (움직이면서 공격할 때 씹힘 방지)
        Physics2D.SyncTransforms();

        Vector2 center = transform.TransformPoint(myCol != null ? myCol.offset : Vector2.zero);

        // [수정] 인스펙터 값이 아닌, Setup에서 계산된 finalRadius를 사용합니다.
        // 공식: $R_{hit} = R_{final} \times |LossyScale.x|$
        float actualDetectionRadius = finalRadius * Mathf.Abs(transform.lossyScale.x);
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = enemyLayer;
        int count = Physics2D.OverlapCircle(center, actualDetectionRadius, filter, hitResults);

        for (int i = 0; i < count; i++)
        {
            GameObject enemy = hitResults[i].gameObject;
            if (hitHistory.Contains(enemy)) continue;

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                int finalDamage = Mathf.RoundToInt(stats.Attack.Value * damageMult);
                Vector2 dir = (enemy.transform.position - owner.transform.position).normalized;

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

        Vector3 center = transform.TransformPoint(myCol.offset);
        // 기즈모도 실제 판정과 똑같은 변수를 사용하도록 설정
        float drawRadius = (finalRadius > 0 ? finalRadius : myCol.radius) * Mathf.Abs(transform.lossyScale.x);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, drawRadius);
    }
}