using UnityEngine;
using Game.Player;

public class BossPoisonProjectile : MonoBehaviour
{
    [Header("독침 설정")]
    public float speed = 12f;
    public float directDamage = 15f; // 맞았을 때 즉시 들어가는 데미지
    [Tooltip("스프라이트 기본 방향 보정 (위쪽을 향해 그려졌다면 -90 입력)")]
    public float rotationOffset = 0f; // [추가] 회전 보정값
    public GameObject puddlePrefab;  // 빗나갔을 때 생성할 독 웅덩이 프리팹

    private Vector2 targetPos;
    private Vector2 moveDir;

    // 보스가 플레이어의 위치를 넘겨주며 발사 방향을 설정함
    public void Setup(Vector2 targetPosition)
    {
        targetPos = targetPosition;
        moveDir = (targetPos - (Vector2)transform.position).normalized;
        
        // 날아가는 방향으로 투사체 회전 (머리가 앞을 향하게)
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
    }

    void Update()
    {
        transform.position += (Vector3)moveDir * speed * Time.deltaTime;

        // 독침이 플레이어가 있던 타겟 위치(바닥)에 도달했다면 (빗나감)
        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            CreatePuddle();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 독침에 플레이어가 직접 맞았다면
        if (col.CompareTag("Player"))
        {
            PlayerStats stats = col.GetComponent<PlayerStats>() ?? col.GetComponentInParent<PlayerStats>() ?? col.transform.root.GetComponentInChildren<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(directDamage); // 즉발 데미지
                
                // 2초 동안 0.5초마다 3의 데미지가 들어가는 독 디버프 부여
                PlayerPoisonDebuff debuff = col.gameObject.GetComponent<PlayerPoisonDebuff>();
                if (debuff == null) debuff = col.gameObject.AddComponent<PlayerPoisonDebuff>();
                debuff.ApplyPoison(2f, 3f); 
            }
            Destroy(gameObject); // 맞췄으므로 웅덩이 생성 없이 즉시 소멸
        }
    }

    private void CreatePuddle()
    {
        if (puddlePrefab != null)
        {
            Instantiate(puddlePrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}