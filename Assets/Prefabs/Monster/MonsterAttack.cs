using UnityEngine;
using Game.Player;

public class MonsterAttack : MonoBehaviour
{
    private EnemyMover mover;
    private Animator anim;
    private EnemyStats stats;

    private Rigidbody2D rb;
    public Transform player;
    public float attackRange;

    private float wanderTimer;
    private Vector2 wanderDirection;
    private bool isWaiting = false;

    public float attackCooldown;
    public float moveSpeed;
    private float lastAttackTime;

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        anim = GetComponent<Animator>();
        if (player == null )
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if(go != null )
            {
                player = go.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || mover == null || anim == null) return;

        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("AttackMotion") ||
                       anim.GetNextAnimatorStateInfo(0).IsName("AttackMotion");

        if (isAttacking)
        {// 공격 중이면 이동을 멈추게 하는 코드
            mover.Stop();
            return; 
        }
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= stats.enemyData.attackRange)
        {
            mover.Stop();
            if (Time.time >= lastAttackTime + stats.enemyData.attackCooldown)
            {
                Attack();
            }
        }
        else if(distance <= stats.detectionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            mover.Move(direction, stats.enemyData.moveSpeed);
        }
        else
        {
            HandlePatrol();
        }
    }

    void HandlePatrol()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            isWaiting = !isWaiting;
            if (!isWaiting)
            {
                wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                wanderTimer = stats.wanderDuration;
            }
            else
            {
                mover.Stop();
                wanderTimer = stats.waitDuration;
            }
        }

        if (!isWaiting)
        {
            mover.Move(wanderDirection, stats.wanderSpeed);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }

    public void OnMonsterHit()
    {
        if (stats == null || player == null)
        {
            return;
        }
        float currentDistance = Vector2.Distance(transform.position, player.position);
        float range = stats.enemyData.attackRange;
        if (currentDistance <= range + 0.5f)
        {
            var pStats = player.GetComponentInChildren<Game.Player.PlayerStats>();
            if (pStats != null)
            {
                float dmg = stats.enemyData.damage;
                pStats.TakeDamage(dmg);
                Debug.Log($"{dmg} 데미지");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 원의 색상을 빨간색으로 설정
        Gizmos.color = Color.red;

        // 현재 위치를 중심으로 attackRange 반지름만큼의 선으로 된 구(원)를 그림
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
