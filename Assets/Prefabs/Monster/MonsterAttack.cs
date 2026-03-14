using UnityEngine;
using Game.Player;

public class MonsterAttack : MonoBehaviour
{
    private EnemyMover mover;
    private Animator anim;
    private EnemyStats stats;
    private MeleeArea meleeArea;
    private EnemyEncounter encounter;

    [HideInInspector]
    private Transform player;

    private float wanderTimer;
    private Vector2 wanderDirection;
    private bool isWaiting = false;
    private float lastAttackTime;

    private Vector3 originalScale;

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        anim = GetComponent<Animator>();
        meleeArea = GetComponentInChildren<MeleeArea>();
        encounter = GetComponent<EnemyEncounter>();

        originalScale = transform.localScale;

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        if (player == null || stats == null || stats.enemyData == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (encounter != null)
        {
            if (encounter.CheckEncounter(distanceToPlayer) || encounter.IsEncountering)
                return;
        }

        if (distanceToPlayer <= stats.enemyData.attackRange)
        {
            FlipTowardsPlayer();
            mover.Stop();
            if (Time.time >= lastAttackTime + stats.enemyData.attackCooldown) Attack();
        }

        else if (distanceToPlayer <= stats.enemyData.detectionRange || (encounter != null && encounter.IsForceChasing))
        {
            FlipTowardsPlayer();
            Vector2 direction = (player.position - transform.position).normalized;

            float currentSpeed = (encounter != null && encounter.IsChasing)
                                 ? stats.enemyData.moveSpeed
                                 : stats.enemyData.wanderSpeed;

            mover.Move(direction, currentSpeed);
        }
        else
        {
            if (encounter != null) encounter.ResetEncounter();
            HandlePatrol();
        }
    }

    void FlipTowardsPlayer()
    {
        float diff = player.position.x - transform.position.x;
        if (Mathf.Abs(diff) < 0.2f) return;
        float directionX = (diff > 0) ? 1f : -1f;
        ApplyFlip(directionX);
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
                if (Mathf.Abs(wanderDirection.x) > 0.1f)
                {
                    float dirX = (wanderDirection.x > 0) ? 1f : -1f;
                    ApplyFlip(dirX);
                }
            }
            else
            {
                mover.Stop();
                wanderTimer = stats.waitDuration;
            }
        }
        if (!isWaiting) mover.Move(wanderDirection, stats.wanderSpeed);
    }

    void ApplyFlip(float x)
    {
        float targetX = Mathf.Abs(originalScale.x) * x;
        if (transform.localScale.x != targetX)
        {
            transform.localScale = new Vector3(targetX, originalScale.y, originalScale.z);
        }
    }

    void Attack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("AttackMotion")) return;
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }

    public void TriggerAttackDamage()
    {
        if (meleeArea != null && stats != null && stats.enemyData != null)
        {
            meleeArea.OnMonsterHit(stats.enemyData.damage);
        }
    }
}