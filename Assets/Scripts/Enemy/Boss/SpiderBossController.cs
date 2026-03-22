using UnityEngine;
using System.Collections;

public enum SpiderBossState
{
    Idle,
    Chase,
    Attack_Melee, // 앞다리로 내려찍기
    Attack_Range, // 거미줄 뱉기
    Dead
}

public class SpiderBossController : MonoBehaviour
{
    [Header("상태 및 타겟")]
    public SpiderBossState currentState;
    public Transform targetPlayer;
    public float moveSpeed = 2f;
    
    [Header("공격 설정")]
    public float meleeAttackRange = 3f;
    public float rangeAttackRange = 8f;
    public float attackCooldown = 3f;
    private float lastAttackTime;

    private EnemyHealth healthScript;

    private void Awake()
    {
        healthScript = GetComponent<EnemyHealth>();
        targetPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (healthScript != null && healthScript.IsDead)
        {
            currentState = SpiderBossState.Dead;
            return;
        }

        switch (currentState)
        {
            case SpiderBossState.Idle:
                // 플레이어를 발견하면 Chase로 전환
                if (targetPlayer != null) currentState = SpiderBossState.Chase;
                break;

            case SpiderBossState.Chase:
                // 플레이어 쪽으로 이동 로직
                Vector2 dir = (targetPlayer.position - transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);

                // 거리에 따라 공격 상태로 전환
                float dist = Vector2.Distance(transform.position, targetPlayer.position);
                if (dist <= meleeAttackRange && Time.time > lastAttackTime + attackCooldown)
                {
                    // TODO: 근접 공격(앞다리 찍기) 코루틴 실행
                }
                break;
        }
    }
}