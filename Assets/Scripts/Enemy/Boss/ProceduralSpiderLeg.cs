using UnityEngine;
using System.Collections;
using Game.Player; // 플레이어 스탯에 접근하기 위해 추가

public class ProceduralSpiderLeg : MonoBehaviour
{
    [Header("다리 설정")]
    public Transform body; // 거미 몸통
    
    public float stepDistance = 1.5f; // 이 거리 이상 멀어지면 발을 뗌
    public float stepSpeed = 10f; // 발을 내딛는 속도
    public float stepHeight = 0.5f; // 발을 들어올리는 높이

    [Header("공격 제한 설정")]
    public float maxReach = 3.5f; // 다리가 몸통에서 분리되지 않도록 뻗을 수 있는 최대 거리 제한

    [Header("긁기 공격(Sweep) 설정")]
    public float sweepWidth = 3.0f; // 부채꼴의 좌우 긁기 넓이 (기본 3.0)
    public float sweepForward = 2.0f; // 긁을 때 앞으로 볼록하게 튀어나오는 거리 (기본 2.0)

    [Header("판정 설정")]
    public float attackRadius = 0.5f; // 공격 판정 반경 (콜라이더 보조용)

    private Vector3 currentPosition; // 현재 발의 위치
    private bool isStepping = false;
    private Vector3 defaultLocalPosition; // 시작 시 다리의 자연스러운 로컬 위치 기억

    // 코루틴 없는 걷기용 상태 (GC 할당 제거)
    private float stepT = -1f;
    private Vector3 stepStartPos;
    
    [Header("체력 설정")]
    public float maxHealth = 50f;
    public float currentHealth;
    public bool isDead = false;
    private SpriteRenderer[] srs; // [수정] 다리 전체의 이미지를 담기 위한 배열

    private EnemyStats bossStats; // [추가] 몸통에서 스탯(데미지)을 가져오기 위한 변수

    private void Start()
    {
        // 1. 게임 시작 시, 거미 몸통을 기준으로 '현재 다리가 있는 위치'를 기억해 둡니다.
        defaultLocalPosition = body.InverseTransformPoint(transform.position);

        // 몸통에 있는 EnemyStats 컴포넌트를 미리 찾아둡니다.
        if (body != null) bossStats = body.GetComponentInParent<EnemyStats>();

        // 2. 다리를 몸통에서 분리하여 독립적으로 움직이게 만듭니다.
        transform.SetParent(null);

        currentPosition = transform.position;

        currentHealth = maxHealth;
        srs = GetComponentsInChildren<SpriteRenderer>(); // [수정] 다리를 구성하는 모든 스프라이트를 가져옴
    }

    private void Update()
    {
        if (body == null) return;

        // 좌우 반전 동기화
        float parentScaleX = body.lossyScale.x;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (parentScaleX < 0 ? -1f : 1f);
        transform.localScale = scale;

        Vector3 targetWorldPos = body.TransformPoint(defaultLocalPosition);

        if (isStepping)
        {
            // 코루틴 없이 Update에서 직접 보간 (GC 할당 없음)
            stepT = Mathf.Min(1f, stepT + Time.deltaTime * stepSpeed);
            currentPosition = Vector3.Lerp(stepStartPos, targetWorldPos, stepT)
                              + new Vector3(0f, Mathf.Sin(stepT * Mathf.PI) * stepHeight, 0f);
            if (stepT >= 1f)
                isStepping = false;
        }
        else if (Vector3.Distance(currentPosition, targetWorldPos) > stepDistance)
        {
            stepStartPos = currentPosition;
            stepT = 0f;
            isStepping = true;
        }

        transform.position = currentPosition;
    }

    // [추가] 다리가 평소 딛고 있어야 할 이상적인 바닥 월드 위치를 반환 (수직 찍기에 사용)
    public Vector3 GetIdealPosition()
    {
        if (body == null) return transform.position;
        return body.TransformPoint(defaultLocalPosition);
    }

    // [추가] 외부에서 다리 공격을 호출할 때 사용하는 메서드 (기존 걷기 코루틴 충돌 방지)
    public void PerformSlam(Vector3 strikePos, bool shakeCamera = false, float slamHeight = 1f, float durationMultiplier = 1f)
    {
        StopAllCoroutines(); // 걷고 있던 동작을 즉시 취소
        StartCoroutine(SlamAttackRoutine(strikePos, shakeCamera, slamHeight, durationMultiplier));
    }

    // [수정] SlamAttack 코루틴 내부 로직
    private IEnumerator SlamAttackRoutine(Vector3 strikePos, bool shakeCamera, float slamHeight, float durationMultiplier)
    {
        if (isDead) yield break;

        isStepping = true; // 찍기 공격 중에는 일반 걷기 로직이 발동하지 않도록 잠금

        // [핵심] 타겟 위치가 몸통에서 너무 멀면, 최대 거리(maxReach)까지만 뻗도록 제한 (다리 분리 방지)
        if (body != null)
        {
            Vector3 offset = strikePos - body.position;
            if (offset.magnitude > maxReach)
            {
                strikePos = body.position + offset.normalized * maxReach;
            }
        }

        Vector3 startPos = currentPosition;

        // 1. 다리를 아주 높이 들어 올림
        Vector3 highPos = startPos + (strikePos - startPos) * 0.5f + (Vector3.up * slamHeight);
        float t = 0;
        while (t < 1f)
        {
            // durationMultiplier로 나눠서 값이 커질수록(예: 1.5) 속도가 느려지게 만듭니다.
            t += Time.deltaTime * ((stepSpeed * 1.5f) / durationMultiplier); 
            currentPosition = Vector3.Lerp(startPos, highPos, t);
            yield return null;
        }

        // 2. 플레이어 위치로 강하게 내려찍기
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * ((stepSpeed * 4f) / durationMultiplier);
            currentPosition = Vector3.Lerp(highPos, strikePos, t);
            yield return null;
        }

        // [추가] 바닥에 닿는 순간 화면 흔들림(Camera Shake) 발생
        if (shakeCamera)
        {
            CameraShake.Instance?.Shake(0.1f, 0.15f); // 강도를 0.1로 대폭 낮춰 살짝만 흔들리게 수정
        }

        // 찍은 상태로 잠시 대기 후 일반 걷기로 복귀
        yield return new WaitForSeconds(0.5f * durationMultiplier); // 찍은 후 대기 시간도 조금 더 길게
        isStepping = false;
    }

    // [추가] 앞다리 전용: 와이퍼처럼 부채꼴로 긁는 공격
    public void PerformSweep(Vector3 targetPos, int directionModifier)
    {
        StopAllCoroutines();
        StartCoroutine(SweepAttackRoutine(targetPos, directionModifier));
    }

    private void ClampToBody(ref Vector3 pos)
    {
        if (body == null) return;
        Vector3 toPos = pos - body.position;
        if (toPos.magnitude > maxReach)
            pos = body.position + toPos.normalized * maxReach;
    }

    private IEnumerator SweepAttackRoutine(Vector3 targetPos, int dirModifier)
    {
        if (isDead) yield break;

        isStepping = true;
        Vector3 startPos = currentPosition;

        if (body == null) yield break;

        Vector3 offset = targetPos - body.position;
        if (offset.magnitude > maxReach)
            targetPos = body.position + offset.normalized * maxReach;

        Vector3 dirToTarget = (targetPos - body.position).normalized;
        dirToTarget.z = 0;

        Vector3 rightTangent = new Vector3(dirToTarget.y, -dirToTarget.x, 0);

        Vector3 sweepStart = targetPos + (rightTangent * dirModifier * sweepWidth) + (Vector3.up * 1.5f);
        Vector3 sweepEnd   = targetPos - (rightTangent * dirModifier * sweepWidth) - (Vector3.up * 0.5f);

        // sweepStart/sweepEnd도 maxReach 이내로 제한 (몸통과 멀어지는 문제 방지)
        ClampToBody(ref sweepStart);
        ClampToBody(ref sweepEnd);

        // 1. 스와이프 시작 위치로 이동
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * (stepSpeed * 1.5f);
            currentPosition = Vector3.Lerp(startPos, sweepStart, t);
            ClampToBody(ref currentPosition);
            yield return null;
        }

        yield return new WaitForSeconds(0.15f);

        // 2. 와이퍼처럼 반대편으로 빠르게 긁기 (부채꼴)
        t = 0;
        bool hitDealt = false; // 스윕 한 번에 중복 피해 방지
        while (t < 1f)
        {
            t += Time.deltaTime * (stepSpeed * 3f);
            Vector3 linearPos = Vector3.Lerp(sweepStart, sweepEnd, t);
            Vector3 arcOffset = dirToTarget * Mathf.Sin(t * Mathf.PI) * sweepForward;
            currentPosition = linearPos + arcOffset;
            ClampToBody(ref currentPosition);

            // 빠른 이동·이미 겹침 모두 커버하는 직접 판정
            if (!hitDealt && !isDead)
            {
                Collider2D hit = Physics2D.OverlapCircle(currentPosition, attackRadius);
                if (hit != null && hit.CompareTag("Player"))
                {
                    TryDealDamage(hit);
                    hitDealt = true;
                }
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        isStepping = false;
    }

    // [추가] 플레이어의 공격을 받았을 때 호출되는 함수
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(HitFeedback()); // 피격 시 붉게 깜빡임

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitFeedback()
    {
        // [수정] 피격 시 다리 전체를 붉게 만듦
        if (srs != null) { foreach(var sr in srs) sr.color = Color.red; }
        
        yield return new WaitForSeconds(0.1f);
        
        if (srs != null && !isDead) 
        { 
            foreach(var sr in srs) sr.color = Color.white; 
        }
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines(); // 진행 중인 모든 움직임과 공격을 즉시 정지

        isStepping = false; // 공격 중 파괴되었을 때, 이어서 바로 다시 걷기 시작할 수 있도록 상태 초기화

        // 파괴된 다리 시각적 연출: 회색으로 변하고 충돌체 비활성화
        // [수정] 다리 전체를 회색으로 만들고 다리에 붙어있는 모든 충돌체를 끔
        if (srs != null) { foreach(var sr in srs) sr.color = Color.gray; } // 일반 회색으로 변경
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach(var col in cols) {
            col.enabled = false; // 충돌체가 꺼지므로 더 이상 플레이어가 닿아도 데미지를 입지 않습니다.
        }
    }

    private void TryDealDamage(Collider2D collision)
    {
        if (bossStats == null || bossStats.enemyData == null || bossStats.enemyData.damage <= 0) return;

        if (!collision.TryGetComponent(out PlayerStats playerStats))
            playerStats = collision.GetComponentInParent<PlayerStats>();
        if (playerStats == null)
            playerStats = collision.transform.root.GetComponentInChildren<PlayerStats>();

        if (playerStats != null)
            playerStats.TakeDamage(bossStats.enemyData.damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;
        if (collision.CompareTag("Player"))
            TryDealDamage(collision);
    }
}