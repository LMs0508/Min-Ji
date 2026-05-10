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
    [Tooltip("패턴 전조 증상 때 숨길 원래 몸통 스프라이트 렌더러")]
    public SpriteRenderer mainBodySprite; 
    [Tooltip("패턴 전조 증상 때 보여줄 붉은색 몸통 게임 오브젝트")]
    public GameObject redBodyVisual;

/* ㅁ차,  */    [Header("절차적 다리 참조")]
    public ProceduralSpiderLeg[] frontLegs; // 공격에 사용할 앞다리들(보통 2개)을 에디터에서 할당
    public ProceduralSpiderLeg[] middleLegs; // 수직으로 내려찍을 중간 다리들 (보통 2개)
    public ProceduralSpiderLeg[] allLegs; // [추가] 보스의 모든 다리 8개를 에디터에서 할당해 주세요
    public float middleLegAttackRange = 3f; // 중간 다리가 플레이어를 감지하는 사거리

    [Header("Phase 2 (몸통 패턴) 설정")]
    public bool isPhase2 = false;
    public GameObject spiderWebObject; // 2페이즈에 등장할 거미줄 게임 오브젝트 (평소엔 꺼둠)
    public Transform webCenterPos; // 거미줄의 중심 위치
    public float webMoveRange = 6f; // 거미줄 위에서 좌우로 이동할 반경
    public bool isTransitioning = false; // [추가] 2페이즈 진입 중 무적 판정을 위한 플래그
    private Vector2 phase2TargetPos;
    private float phase2MoveTimer;

    [Header("Phase 2 독침 공격 설정")]
    public GameObject poisonProjectilePrefab; // 독침 프리팹
    public Transform poisonSpawnPoint; // [추가] 독침이 발사될 위치 (보스의 입)
    public float poisonSpitCooldown = 4f; // 독침 뱉기 쿨타임
    private float lastPoisonSpitTime;

    [Header("사망 연출 설정")]
    public GameObject deathVisual; // 죽을 때 켜질 죽음 애니메이션 오브젝트

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
            // 죽음 처리를 한 번만 실행하기 위한 조건문
            if (currentState != SpiderBossState.Dead)
            {
                currentState = SpiderBossState.Dead;
                HandleDeath(); // 몸통 끄고 시체 켜기
            }
            return;
        }

        // [추가] 페이즈 2 진입 조건 체크: 다리가 모두 부서졌고, 아직 2페이즈가 아닐 때
        if (currentState != SpiderBossState.Dead && !isPhase2 && !isTransitioning && AreAllLegsDestroyed())
        {
            StartCoroutine(EnterPhase2Routine());
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

                if (isPhase2)
                {
                    // [2페이즈] 독침 뱉기 쿨타임 체크 (가장 먼저 검사)
                    if (Time.time > lastPoisonSpitTime + poisonSpitCooldown && poisonProjectilePrefab != null)
                    {
                        StartCoroutine(PoisonSpitRoutine());
                        return;
                    }

                    // [2페이즈] 거미줄 위에서 좌우로 랜덤 이동
                    transform.position = Vector2.MoveTowards(transform.position, phase2TargetPos, stats.moveSpeed * Time.deltaTime);
                    
                    // 목표 위치에 도달했거나 타이머가 지나면 새 위치로 이동
                    if (Vector2.Distance(transform.position, phase2TargetPos) < 0.1f || Time.time > phase2MoveTimer)
                    {
                        SetNewPhase2Target();
                    }
                }
                else
                {
                    // [1페이즈] 다리 근접 공격 및 추적
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
                }
                break;
        }
    }

    // [추가] 보스 사망 시 시각적 연출 처리 (EnemyHealth에서 직접 호출할 수 있도록 public으로 변경)
    public void HandleDeath()
    {
        StopAllCoroutines();

        // 1. 본체(자식 포함)에 붙어있는 모든 스프라이트 렌더러 끄기
        SpriteRenderer[] allSrs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var s in allSrs) s.enabled = false;

        // 2. 몸통에서 분리되어 돌아다니던 다리들(8개)의 스프라이트도 끄기
        if (allLegs != null)
        {
            foreach (var leg in allLegs)
            {
                if (leg != null)
                {
                    SpriteRenderer[] legSrs = leg.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var s in legSrs) s.enabled = false;
                }
            }
        }

        // 3. 거미줄 끄기 (보스가 죽어도 거미줄을 남기려면 이 부분을 주석 처리합니다)
        // if (spiderWebObject != null) spiderWebObject.SetActive(false);

        // 4. 죽음 전용 애니메이션 오브젝트 켜기
        if (deathVisual != null)
        {
            deathVisual.SetActive(true);
            SpriteRenderer[] deathSrs = deathVisual.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var s in deathSrs) s.enabled = true; // 방금 위에서 껐으므로 다시 켜줍니다.
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

    // [추가] 본체 무적 여부 확인 (다리가 살아있거나, 2페이즈 이동 중일 때)
    public bool IsInvincible()
    {
        return !AreAllLegsDestroyed() || isTransitioning;
    }

    // [추가] 2페이즈 진입 연출 코루틴
    private IEnumerator EnterPhase2Routine()
    {
        currentState = SpiderBossState.Idle; // 이동 및 공격 임시 정지
        isTransitioning = true; // 이동 중 무적

        // 1. 거미줄 오브젝트 활성화
        if (spiderWebObject != null) spiderWebObject.SetActive(true);

        // 2. 거미줄 중심 위치로 보스 몸통 걸어가기
        if (webCenterPos != null)
        {
            // 목표 지점에 도달할 때까지 반복
            while (Vector2.Distance(transform.position, webCenterPos.position) > 0.1f)
            {
                // 원래 이동 속도보다 4배 빠르게 중앙으로 빠릿하게 걸어감
                transform.position = Vector2.MoveTowards(transform.position, webCenterPos.position, stats.moveSpeed * 4f * Time.deltaTime);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f); // 중앙 도착 후 잠시 대기 (긴장감)

        isTransitioning = false; // 무적 해제
        isPhase2 = true; // 2페이즈 공격 시작
        SetNewPhase2Target(); // 첫 번째 랜덤 목표 설정
        currentState = SpiderBossState.Chase; // 다시 움직임 시작 (이제 2페이즈 로직을 따름)
    }

    // [추가] 2페이즈 거미줄 위 랜덤 이동 목표 설정
    private void SetNewPhase2Target()
    {
        if (webCenterPos == null) return;
        float randomX = Random.Range(webCenterPos.position.x - webMoveRange, webCenterPos.position.x + webMoveRange);
        phase2TargetPos = new Vector2(randomX, webCenterPos.position.y); // Y축은 거미줄 높이로 고정
        phase2MoveTimer = Time.time + Random.Range(2f, 4f); // 2~4초마다 목표 변경
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
        if (mainBodySprite != null && redBodyVisual != null)
        {
            mainBodySprite.enabled = false; // 원래 몸통 숨기기
            redBodyVisual.SetActive(true);  // 붉은 몸통 보이기
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
        if (mainBodySprite != null && redBodyVisual != null)
        {
            redBodyVisual.SetActive(false); // 붉은 몸통 숨기기
            mainBodySprite.enabled = true;   // 원래 몸통 다시 보이기
        }
        currentState = SpiderBossState.Chase;
    }

    // [추가] 2페이즈 전용 독침 뱉기 패턴 코루틴
    private IEnumerator PoisonSpitRoutine()
    {
        currentState = SpiderBossState.Attack_Range;
        lastPoisonSpitTime = Time.time;
        lastAttackTime = Time.time; // 다른 공격과 동기화

        // 발사 전 0.4초 딜레이 (기를 모으거나 입을 벌리는 연출)
        yield return new WaitForSeconds(0.4f);

        if (targetPlayer != null && poisonProjectilePrefab != null)
        {
            Vector3 spawnPos = poisonSpawnPoint != null ? poisonSpawnPoint.position : transform.position;
            GameObject proj = Instantiate(poisonProjectilePrefab, spawnPos, Quaternion.identity);
            BossPoisonProjectile script = proj.GetComponent<BossPoisonProjectile>();
            if (script != null) script.Setup(targetPlayer.position);
        }

        // 발사 후 0.6초 딜레이
        yield return new WaitForSeconds(0.6f);
        currentState = SpiderBossState.Chase;
    }
}