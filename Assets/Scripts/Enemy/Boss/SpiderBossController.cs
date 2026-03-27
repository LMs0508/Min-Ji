using UnityEngine;
using System.Collections;

public enum SpiderBossState
{
    Idle,
    Chase,
    Attack_Melee, // 앞다리로 내려찍기
    Attack_Range, // 거미줄 뱉기
    Attack_ScreenSlash, // [추가] 화면 가르기 (유리창 깨지는 느낌)
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

    [Header("화면 가르기(광선) 공격 설정")]
    public GameObject screenSlashPrefab; // 얇은 선 -> 굵은 광선이 되는 프리팹
    public int slashCount = 4; // 한 번에 그을 광선 갯수
    public float slashCooldown = 10f; // 광선 공격 쿨타임
    private float lastSlashTime;
    public SpriteRenderer bossEyes; // [옵션] 붉게 빛날 보스의 눈 스프라이트

/* ㅁ차,  */    [Header("절차적 다리 참조")]
    public ProceduralSpiderLeg[] frontLegs; // 공격에 사용할 앞다리들(보통 2개)을 에디터에서 할당
    public ProceduralSpiderLeg[] middleLegs; // 수직으로 내려찍을 중간 다리들 (보통 2개)
    public ProceduralSpiderLeg[] allLegs; // [추가] 보스의 모든 다리 8개를 에디터에서 할당해 주세요
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

                // 0. 화면 가르기 공격 쿨타임 체크 (가장 우선순위가 높게 설정)
                // 사거리 무관하게 화면 전체 공격이므로 바로 발동합니다.
                if (Time.time > lastSlashTime + slashCooldown && screenSlashPrefab != null)
                {
                    StartCoroutine(ScreenSlashRoutine());
                    return;
                }

                // 1. 앞다리들 중 하나라도 플레이어가 사거리 내에 들어왔는지 체크 (다리 위치 기준)
                bool canFrontAttack = false;
                foreach (var leg in frontLegs)
                {
                    // 살아있는 다리만 공격 범위 체크
                    if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
                    {
                        canFrontAttack = true;
                        break;
                    }
                }

                // 2. 중간 다리들 중 하나라도 플레이어가 사거리 내에 들어왔는지 체크
                bool canMiddleAttack = false;
                foreach (var leg in middleLegs)
                {
                    if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= middleLegAttackRange)
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

    // [추가] 본체 공격 가능 여부를 판단하기 위해, 모든 다리가 부서졌는지 확인
    public bool AreAllLegsDestroyed()
    {
        if (allLegs == null || allLegs.Length == 0) return true; // 다리가 등록 안 되어있으면 무적 아님
        foreach (var leg in allLegs)
        {
            if (leg != null && !leg.isDead) 
                return false; // 하나라도 살아있으면 아직 무적
        }
        return true; // 모두 파괴됨
    }

    private IEnumerator MeleeAttackRoutine()
    {
        currentState = SpiderBossState.Attack_Melee; // 몸체의 추적 이동을 멈춤
        lastAttackTime = Time.time; // 공격을 시작할 때 바로 쿨타임 계산을 시작합니다.

        // 1. 앞다리 사거리 내에 있을 때, 가장 가까운 앞다리 하나를 골라 와이퍼(부채꼴) 긁기
        bool canFrontAttack = false;
        foreach (var leg in frontLegs)
        {
            if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
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
                if (leg == null || leg.isDead) continue; // 부서진 다리는 제외
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
            if (leg != null && !leg.isDead) // 살아있는 다리만 내려찍기 수행
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

    // [추가] 화면 가르기 (유리창 깨지는 광선) 패턴 코루틴
    private IEnumerator ScreenSlashRoutine()
    {
        currentState = SpiderBossState.Attack_ScreenSlash;
        lastSlashTime = Time.time;
        lastAttackTime = Time.time; // 다른 근접 공격과 겹치지 않게 쿨타임 동기화

        // 1. 전조 증상: 보스 눈이 붉어짐 (bossEyes가 할당되어 있을 경우)
        Color originalEyeColor = Color.white;
        if (bossEyes != null) {
            originalEyeColor = bossEyes.color;
            bossEyes.color = Color.red;
        }

        yield return new WaitForSeconds(0.5f); // 눈이 빛나고 잠시 뜸 들이기

        // 2. 플레이어 주변/화면 곳곳에 랜덤한 각도와 위치로 광선(경고선) 프리팹 배치
        for (int i = 0; i < slashCount; i++)
        {
            // 플레이어 근처 랜덤 위치 생성
            Vector2 randomPos = (Vector2)targetPlayer.position + new Vector2(Random.Range(-6f, 6f), Random.Range(-4f, 4f));
            
            // 각도를 지그재그, 가로세로 교차 느낌으로 다양하게 줍니다.
            float randomAngle = Random.Range(-20f, 20f); 
            if (i % 2 == 0) randomAngle += Random.Range(70f, 110f); // 짝수번째는 거의 세로/대각선으로 긋게 설정

            Instantiate(screenSlashPrefab, randomPos, Quaternion.Euler(0, 0, randomAngle));
        }

        // 광선이 터지고 끝날 때까지 넉넉히 대기
        yield return new WaitForSeconds(1.5f);

        // 눈 색깔 원상복구
        if (bossEyes != null) bossEyes.color = originalEyeColor;
        currentState = SpiderBossState.Chase;
    }
}