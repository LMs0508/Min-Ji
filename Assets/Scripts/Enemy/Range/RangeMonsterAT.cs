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

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        anim = GetComponent<Animator>();

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                player = go.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || mover == null || stats == null || anim == null)
            return;
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attacking") ||
           anim.GetNextAnimatorStateInfo(0).IsTag("Attacking"))
        {
            mover.Stop();
            return;
        }
        float distance = Vector2.Distance(transform.position, player.position);
        var data = stats.enemyData;

        if (distance > data.attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            mover.Move(direction, data.moveSpeed);
        }
        else if (distance < data.stopDistance)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            mover.Move(direction, data.moveSpeed);
        }
        else
        {
            mover.Stop();
            if (Time.time >= lastAttackTime + data.attackCooldown)
                ShootTrigger();
        }
    }
    
    void ShootTrigger()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }
    
    public void OnMonsterShoot()
    {
        if(projectilePrefab != null && firePoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 shootDir = (player.position - firePoint.position).normalized;

            ProjectileLogic projScript = proj.GetComponent<ProjectileLogic>();
            if (projScript != null)
            {
                projScript.Setup(shootDir, stats.enemyData.damage);
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
