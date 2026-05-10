using UnityEngine;

public class RangeMonsterAT : MonoBehaviour
{
    private EnemyMover mover;
    private Animator anim;
    private EnemyStats stats;
    private EnemyHealth health;
    private EnemyEncounter encounter;

    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;

    private float lastAttackTime;
    private float wanderTimer;
    private Vector2 wanderDirection;
    private bool isWaiting = false;

    private enum CombatPhase { Idle, Relocating, Attacking, Freezing }
    private CombatPhase currentPhase = CombatPhase.Idle;

    private int attackCount = 0;
    private float freezeTimer = 0f;
    private Vector3 originalScale;

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        health = GetComponentInParent<EnemyHealth>();
        encounter = GetComponentInChildren<EnemyEncounter>();

        GameObject go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
    }

    void Update()
    {
        if (player == null || mover == null || stats == null || anim == null) return;

        if (health != null && health.IsDead) return;

        if (encounter != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            encounter.CheckEncounter(distToPlayer);
        }

        if (encounter != null && encounter.IsEncountering)
        {
            mover.Stop();
            return;
        }

        // 1. 0.05초 재판단 정지 상태
        if (currentPhase == CombatPhase.Freezing)
        {
            mover.Stop();
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0) currentPhase = CombatPhase.Idle;
            return;
        }

        // 2. 공격 애니메이션 중 이동 차단
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attacking"))
        {
            mover.Stop();
            FaceTarget(player.position.x - transform.position.x);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        var data = stats.enemyData;

        if (distance <= stats.detectionRange)
            HandleCombatPhase(distance, data);
        else
            HandlePatrol(data);
    }

    private void HandleCombatPhase(float distance, EnemyData data)
    {
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        Vector2 forward = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
        float angle = Vector2.Angle(forward, dirToPlayer);

        // [Phase: Idle] 공격 세션 종료 후 재판단
        if (currentPhase == CombatPhase.Idle)
        {
            // 도망 로직 완전 삭제. 
            // 1. 사거리 밖이거나 2. 사격 각도(22.5도) 밖일 때만 움직임
            if (distance > data.attackRange + 0.5f || angle > 22.5f)
            {
                currentPhase = CombatPhase.Relocating;
            }
            else
            {
                attackCount = 0;
                currentPhase = CombatPhase.Attacking;
            }
        }

        // [Phase: Relocating] 조건 충족을 위한 최소한의 이동
        else if (currentPhase == CombatPhase.Relocating)
        {
            // 이동 방향 결정
            if (distance > data.attackRange)
            {
                // 너무 멀면 플레이어 쪽으로 이동
                mover.Move(dirToPlayer, data.moveSpeed);
                FaceTarget(dirToPlayer.x);
            }
            else if (angle > 22.5f)
            {
                // 사거리 안이지만 각도가 안 맞으면 높이(Y축) 조절
                float yDir = (player.position.y > transform.position.y) ? 1f : -1f;
                mover.Move(new Vector2(0, yDir), data.moveSpeed);
                FaceTarget(player.position.x - transform.position.x); // 조준하며 이동
            }

            // 이동 중 실시간 체크: 범위 안 + 각도 안이면 즉시 공격 단계로 전환
            if (distance <= data.attackRange && angle <= 22.5f)
            {
                mover.Stop();
                attackCount = 0;
                currentPhase = CombatPhase.Attacking;
            }
        }

        // [Phase: Attacking] 멈춰서 공격 (2회)
        else if (currentPhase == CombatPhase.Attacking)
        {
            mover.Stop();
            FaceTarget(player.position.x - transform.position.x);

            if (Time.time >= lastAttackTime + data.attackCooldown)
                ShootTrigger();
        }
    }

    public void OnMonsterShoot()
    {
        if (projectilePrefab != null && firePoint != null && player != null)
        {
            Vector2 targetDir = (player.position - firePoint.position).normalized;
            Vector2 forward = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;

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

            attackCount++;
            if (attackCount >= 2)
            {
                attackCount = 0;
                freezeTimer = 0.05f;
                currentPhase = CombatPhase.Freezing;
            }
        }
    }

    private void FaceTarget(float xDiff)
    {
        if (health != null && health.IsDead) return;
        if (encounter != null && encounter.IsEncountering) return;

        if (Mathf.Abs(xDiff) < 0.1f) return;
        float targetX = (xDiff > 0) ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * targetX, originalScale.y, originalScale.z);
    }

    void ShootTrigger() { lastAttackTime = Time.time; anim.SetTrigger("Attack"); }

    private void HandlePatrol(EnemyData data)
    {
        currentPhase = CombatPhase.Idle;
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0)
        {
            isWaiting = !isWaiting;
            if (!isWaiting)
            {
                wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                wanderTimer = data.wanderDuration;
            }
            else { mover.Stop(); wanderTimer = data.waitDuration; }
        }
        if (!isWaiting)
        {
            mover.Move(wanderDirection, data.moveSpeed);
            FaceTarget(wanderDirection.x);
        }
    }
}