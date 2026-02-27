using UnityEngine;

public class WaterWhirlwind : MonoBehaviour
{
    public int damage = 3;
    public float damageInterval = 0.1f;
    public float pullForce = 2f;
    private float timer;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 1. 데미지 로직
            timer += Time.deltaTime;
            if (timer >= damageInterval)
            {
                EnemyHealth health = collision.GetComponent<EnemyHealth>();
                if (health != null) health.TakeDamage(damage);
                timer = 0f;
            }

            // 2. [특수 효과] 적을 회오리 중앙으로 살짝 끌어당김
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 pullDir = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pullDir * pullForce);
            }
        }
    }
}