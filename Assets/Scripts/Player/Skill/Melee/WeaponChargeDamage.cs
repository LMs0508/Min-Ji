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

    // [�߰�] �ڵ忡�� ������ ������ ����
    private float finalRadius;

    private Collider2D[] hitResults = new Collider2D[20]; // �迭 ũ�⸦ 20���� Ȯ�� (����ȭ)
    private HashSet<GameObject> hitHistory = new HashSet<GameObject>();

    private void Awake()
    {
        myCol = GetComponent<CircleCollider2D>();
    }

    // [����] baseRadius�� ���ڷ� �޽��ϴ�.
    public void Setup(GameObject owner, PlayerStats stats, float mult, float kb, LayerMask layer, float radius)
    {
        this.owner = owner;
        this.stats = stats;
        this.damageMult = mult;
        this.knockback = kb;
        this.enemyLayer = layer;
        this.finalRadius = radius; // ���޹��� ���� ������ ����

        if (myCol == null) myCol = GetComponent<CircleCollider2D>();
        hitHistory.Clear();
    }

    public void CheckHit()
    {
        if (stats == null) return;

        // [����ȭ] ���� ���� ����ȭ (�����̸鼭 ������ �� ���� ����)
        Physics2D.SyncTransforms();

        Vector2 center = transform.TransformPoint(myCol != null ? myCol.offset : Vector2.zero);

        // [����] �ν����� ���� �ƴ�, Setup���� ���� finalRadius�� ����մϴ�.
        // ����: $R_{hit} = R_{final} \times |LossyScale.x|$
        float actualDetectionRadius = finalRadius * Mathf.Abs(transform.lossyScale.x);
        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = enemyLayer;
        int count = Physics2D.OverlapCircle(center, actualDetectionRadius, filter, hitResults);

        for (int i = 0; i < count; i++)
        {
            GameObject enemy = hitResults[i].gameObject;
            if (hitHistory.Contains(enemy)) continue;

            EnemyHealth health = enemy.GetComponentInParent<EnemyHealth>();
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
        // ����� ���� ������ �Ȱ��� ������ ����ϵ��� ����
        float drawRadius = (finalRadius > 0 ? finalRadius : myCol.radius) * Mathf.Abs(transform.lossyScale.x);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, drawRadius);
    }
}