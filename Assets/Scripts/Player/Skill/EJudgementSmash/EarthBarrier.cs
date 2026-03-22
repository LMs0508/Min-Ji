using UnityEngine;

public class EarthBarrier : MonoBehaviour
{
    [Header("위치 설정")]
    public Vector3 positionOffset = new Vector3(0, 0.5f, 0);

    [Header("데미지 설정")]
    public int damage = 5;
    public float damageInterval = 0.1f;
    private float timer;

    private void Start()
    {
        transform.localPosition = positionOffset;
    }

    private void LateUpdate()
    {
        transform.localPosition = positionOffset;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            timer += Time.deltaTime;
            if (timer >= damageInterval)
            {
                EnemyHealth health = collision.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                    health.TakeDamage(damage, pushDir);
                }
                timer = 0f;
            }
        }
    }
}