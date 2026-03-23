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
    
    [Header("공격 설정")]
    public float rangeAttackRange = 8f;
    private float lastAttackTime;

/* ㅁ차,  */    [Header("절차적 다리 참조")]
    public ProceduralSpiderLeg[] frontLegs; // 공격에 사용할 앞다리들(보통 2개)을 에디터에서 할당
    public ProceduralSpiderLeg[] middleLegs; // 수직으로 내려찍을 중간 다리들 (보통 2개)
    public float middleLegAttackRange = 3f; // 중간 다리가 플레이어를 감지하는 사거리

    private EnemyHealth healthScript;
    private EnemyStats stats;

    private void Awake()
    {
        healthScript = GetComponent<EnemyHealth>();
        stats = GetComponent<EnemyStats>();
        targetPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        // 스탯 데이터가 없으면 오류를 방지하기 위해 작동 중지
        if (stats == null || stats.enemyData == null) return;

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

                // 1. 앞다리들 중 하나라도 플레이어가 사거리 내에 들어왔는지 체크 (다리 위치 기준)
                bool canFrontAttack = false;
                foreach (var leg in frontLegs)
                {
                    // EnemyData의 공격 사거리(attackRange)를 앞다리 사거리로 사용
                    if (leg != null && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
                    {
                        canFrontAttack = true;
                        break;
                    }
                }

                // 2. 중간 다리들 중 하나라도 플레이어가 사거리 내에 들어왔는지 체크
                bool canMiddleAttack = false;
                foreach (var leg in middleLegs)
                {
                    if (leg != null && Vector2.Distance(leg.transform.position, targetPlayer.position) <= middleLegAttackRange)
                    {
                        canMiddleAttack = true;
                        break;
                    }
                }

                // 3. 앞다리나 중간 다리 사거리 내 라면 공격 시작
                if ((canFrontAttack || canMiddleAttack) && Time.time > lastAttackTime + stats.enemyData.attackCooldown)
                {
                    StartCoroutine(MeleeAttackRoutine());
                }
                else
                {
                    // 4. 어떤 다리 공격 사거리에도 닿지 않을 때만 플레이어 쪽으로 이동 (사거리 진입 시 즉시 멈춤)
                    transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, stats.moveSpeed * Time.deltaTime);
                }
                break;
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        currentState = SpiderBossState.Attack_Melee; // 몸체의 추적 이동을 멈춤
        lastAttackTime = Time.time; // 공격을 시작할 때 바로 쿨타임 계산을 시작합니다.

        // 1. 앞다리 사거리 내에 있을 때, 가장 가까운 앞다리 하나를 골라 와이퍼(부채꼴) 긁기
        bool canFrontAttack = false;
        foreach (var leg in frontLegs)
        {
            if (leg != null && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
            {
                canFrontAttack = true;
                break;
            }
        }

        if (canFrontAttack)
        {
            Vector3 strikeTarget = targetPlayer.position;
            ProceduralSpiderLeg closestLeg = null;
            float minDistance = float.MaxValue;

            // 플레이어와 가장 가까운 다리 찾기
            foreach (var leg in frontLegs)
            {
                if (leg == null) continue;
                float d = Vector2.Distance(leg.transform.position, strikeTarget);
                if (d < minDistance) { minDistance = d; closestLeg = leg; }
            }

            if (closestLeg != null)
            {
                // 몸통 기준 다리의 상대 위치(X좌표)를 통해 왼쪽/오른쪽 판별
                float localX = transform.InverseTransformPoint(closestLeg.transform.position).x;
                int sweepDir = localX > 0 ? 1 : -1; // 1이면 우측다리(우->좌 스와이프), -1이면 좌측다리(좌->우 스와이프)
                
                closestLeg.PerformSweep(strikeTarget, sweepDir);
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
                    leg.PerformSlam(verticalTarget, true, 2f, 2f);
                }
            }
        }

        // 공격 동작(들어올리고 찍고 대기)이 끝날 때까지 대기
        yield return new WaitForSeconds(1.5f);

        currentState = SpiderBossState.Chase; // 다시 플레이어를 추적하는 상태로 복귀
    }
}