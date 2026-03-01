using UnityEngine;
using Game.Player;

public class MonsterAttack : MonoBehaviour
{
    private EnemyMover mover;
    private Animator anim;
    private EnemyStats stats;
    private MeleeArea meleeArea;

    public Transform player;

    private float wanderTimer;
    private Vector2 wanderDirection;
    private bool isWaiting = false;
    private float lastAttackTime;

    // [수정] 최상위 부모가 아닌, 이 스크립트(Visuals)의 원래 크기를 저장합니다.
    private Vector3 originalScale;

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        anim = GetComponent<Animator>();
        meleeArea = GetComponentInChildren<MeleeArea>();

        // [수정] transform.root 대신 현재 오브젝트(Visuals)의 스케일을 저장
        originalScale = transform.localScale;

        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    void Update()
    {
        if (player == null || mover == null || anim == null || stats == null || stats.enemyData == null) return;

        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("AttackMotion") ||
                           anim.GetNextAnimatorStateInfo(0).IsName("AttackMotion");

        if (isAttacking)
        {
            // 공격 중에는 방향을 바꾸지 않음
            mover.Stop();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        float currentRange = (meleeArea != null) ? meleeArea.radius : 1.0f;

        if (distance <= currentRange)
        {
            FlipTowardsPlayer();
            mover.Stop();
            if (Time.time >= lastAttackTime + stats.enemyData.attackCooldown)
            {
                Attack();
            }
        }
        else if (distance <= stats.detectionRange)
        {
            FlipTowardsPlayer(); // 걷는 도중에도 방향 전환 실행
            Vector2 direction = (player.position - transform.position).normalized;
            mover.Move(direction, stats.enemyData.moveSpeed);
        }
        else
        {
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

        if (!isWaiting)
        {
            mover.Move(wanderDirection, stats.wanderSpeed);
        }
    }

    void ApplyFlip(float x)
    {
        // [수정] transform.root 대신 transform(Visuals)의 스케일만 변경
        float targetX = Mathf.Abs(originalScale.x) * x;

        if (transform.localScale.x != targetX)
        {
            transform.localScale = new Vector3(targetX, originalScale.y, originalScale.z);
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }
}