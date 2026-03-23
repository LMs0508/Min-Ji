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
                // [핵심 수정] 보스처럼 충돌체가 자식에 있는 경우를 위해 부모에서 스크립트를 찾도록 변경합니다!
                EnemyHealth health = collision.GetComponentInParent<EnemyHealth>();
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