using UnityEngine;

public class RangeMonsterAT : MonoBehaviour
{
    private EnemyMover mover;
    private Animator anim;
    private EnemyStats stats;

    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float lastAttackTime;

    private float wanderTimer;
    private Vector2 wanderDirection;
    private bool isWaiting = false;

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        anim = GetComponent<Animator>();

        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            player = go.transform;
        }
    }

    void Update()
    {
        if (player == null || mover == null || stats == null || anim == null)
            return;

        if (player == null) { Debug.LogWarning("플레이어 태그를 못 찾았어요!"); return; }

        bool attacking = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attacking");

        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attacking"))
        {
            mover.Stop();
            return;
        }
        float distance = (player != null) ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
        var data = stats.enemyData;

        if (player != null && distance <= stats.detectionRange)
        {
            HandleCombat(distance, data);
        }
        else
        {
            HandlePatrol(data);
        }
    }

    private void HandleCombat(float distance, EnemyData data)
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float buffer = 0.5f;

        if (distance > data.attackRange + buffer)
        {
            mover.Move(direction, data.moveSpeed);
        }
        else if (distance < data.stopDistance - buffer)
        {
            mover.Move(-direction, data.moveSpeed);
        }
        else
        {
            mover.Stop();
            mover.LookAt(direction);

            if (Time.time >= lastAttackTime + data.attackCooldown)
            {
                ShootTrigger();
            }
        }
    }

    private void HandlePatrol(EnemyData data)
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            isWaiting = !isWaiting;
            if (!isWaiting)
            {
                wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                wanderTimer = data.wanderDuration;
            }
            else
            {
                mover.Stop();
                wanderTimer = data.waitDuration;
            }
        }

        if (!isWaiting)
        {
            mover.Move(wanderDirection, data.moveSpeed);
        }
    }

    void ShootTrigger()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }
    
    public void OnMonsterShoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Vector2 targetDir = (player.position - firePoint.position).normalized; // 플레이어 향한 방향
            Vector2 forward = (transform.lossyScale.x > 0) ? Vector2.right : Vector2.left; // 몬스터의 현재 정면
            float signedAngle = Vector2.SignedAngle(forward, targetDir);
            float clampedAngle = Mathf.Clamp(signedAngle, -22.5f, 22.5f);

            Quaternion rotation = Quaternion.Euler(0, 0, clampedAngle);
            Vector2 finalShootDir = (rotation * forward).normalized;

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            ProjectileLogic projScript = proj.GetComponent<ProjectileLogic>();
            if (projScript != null)
            {
                projScript.Setup(finalShootDir, stats.enemyData.damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null) stats = GetComponentInParent<EnemyStats>();
        if (stats != null && stats.enemyData != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, stats.enemyData.attackRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.enemyData.stopDistance);
        }
    }
}
