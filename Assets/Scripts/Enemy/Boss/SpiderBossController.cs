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

/* ㅁ차,  */    [Header("절차적 다리 참조")]
    public ProceduralSpiderLeg[] frontLegs; // 공격에 사용할 앞다리들(보통 2개)을 에디터에서 할당
    public ProceduralSpiderLeg[] middleLegs; // 수직으로 내려찍을 중간 다리들 (보통 2개)
    public float middleLegAttackRange = 3f; // 중간 다리가 플레이어를 감지하는 사거리

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
                if (targetPlayer == null) return;

                float distToBody = Vector2.Distance(transform.position, targetPlayer.position);
                
                // 1. 중간 다리들 중 하나라도 플레이어가 사거리 내에 들어왔는지 체크
                bool canMiddleAttack = false;
                foreach (var leg in middleLegs)
                {
                    if (leg != null && Vector2.Distance(leg.transform.position, targetPlayer.position) <= middleLegAttackRange)
                    {
                        canMiddleAttack = true;
                        break;
                    }
                }

                // 2. 앞다리 사거리 내 이거나, 중간 다리 사거리 내 라면 공격 시작
                if ((distToBody <= meleeAttackRange || canMiddleAttack) && Time.time > lastAttackTime + attackCooldown)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    // 3. 어떤 다리 공격 사거리에도 닿지 않을 때만 플레이어 쪽으로 이동 (사거리 진입 시 즉시 멈춤)
                    transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);
                }
                break;
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        currentState = SpiderBossState.Attack_Melee; // 몸체의 추적 이동을 멈춤

        // 1. 플레이어가 앞다리 사거리 내에 있을 때만 정면으로 모여서 찌름
        float distToBody = Vector2.Distance(transform.position, targetPlayer.position);
        if (distToBody <= meleeAttackRange)
        {
            Vector3 strikeTarget = targetPlayer.position;
            foreach (var leg in frontLegs)
            {
                if (leg != null) leg.PerformSlam(strikeTarget, false, 1f, 1f); // 앞다리는 기본 높이 1, 길이 배율 1
            }
        }

        // 2. 중간 다리들은 플레이어가 자기 근처에 있으면 제자리(수직 바닥)를 강하게 찍음
        foreach (var leg in middleLegs)
        {
            if (leg != null)
            {
                // 발끝 기준이 아니라 다리의 현재 위치(스프라이트 기준)로 플레이어와의 거리를 잼
                float distToLeg = Vector2.Distance(leg.transform.position, targetPlayer.position);
                if (distToLeg <= middleLegAttackRange)
                {
                    // 자기 원래 발 위치를 향해 강하게 수직으로 꽂음
                    Vector3 verticalTarget = leg.GetIdealPosition();
                    // 중간 다리는 흔들림 발생, 높이 1.5, 애니메이션 길이를 1.5배 길게 늘림!
                    leg.PerformSlam(verticalTarget, true, 1.5f, 2f);
                }
            }
        }

        // 공격 동작(들어올리고 찍고 대기)이 끝날 때까지 대기
        yield return new WaitForSeconds(1.5f);

        lastAttackTime = Time.time;
        currentState = SpiderBossState.Chase; // 다시 플레이어를 추적하는 상태로 복귀
    }
}