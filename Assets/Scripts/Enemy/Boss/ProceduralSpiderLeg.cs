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

    private Vector3 currentPosition; // 현재 발의 위치
    private bool isStepping = false;
    private Vector3 defaultLocalPosition; // 시작 시 다리의 자연스러운 로컬 위치 기억
    
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
    }

    private void Update()
    {
        transform.position = currentPosition;

        // [추가] 몸통에서 분리되었으므로, 혹시라도 거미 몸통이 좌우로 반전(Flip)된다면 
        // 다리의 이미지도 올바른 방향을 바라보도록 스케일을 동기화해 줍니다.
        if (body != null)
        {
            float parentScaleX = body.lossyScale.x;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (parentScaleX < 0 ? -1f : 1f);
            transform.localScale = scale;
        }

        // 3. 매 프레임 몸통의 이동에 따라 다리가 있어야 할 '목표 월드 위치'를 계산합니다.
        Vector3 targetWorldPos = body.TransformPoint(defaultLocalPosition);

        // 발이 목표 위치에서 너무 멀어지면 새로운 걸음(Step)을 시작함
        if (!isStepping && Vector3.Distance(currentPosition, targetWorldPos) > stepDistance)
        {
            StartCoroutine(Step());
        }
    }

    private IEnumerator Step()
    {
        isStepping = true;
        Vector3 startPos = currentPosition;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * stepSpeed;
            // 몸통이 움직이는 중에도 발이 정확히 따라가도록 도착 지점을 계속 갱신합니다.
            Vector3 targetPos = body.TransformPoint(defaultLocalPosition);
            currentPosition = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * stepHeight;
            yield return null;
        }
        isStepping = false;
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

    private IEnumerator SweepAttackRoutine(Vector3 targetPos, int dirModifier)
    {
        isStepping = true;
        Vector3 startPos = currentPosition;

        if (body == null) yield break;

        // [핵심] 긁기 공격 시에도 다리가 너무 멀리 나가지 않도록 거리 제한
        Vector3 offset = targetPos - body.position;
        if (offset.magnitude > maxReach)
        {
            targetPos = body.position + offset.normalized * maxReach;
        }

        // 타겟을 향하는 방향
        Vector3 dirToTarget = (targetPos - body.position).normalized;
        dirToTarget.z = 0;

        // 타겟 방향에 수직인 벡터 (오른쪽 방향)
        Vector3 rightTangent = new Vector3(dirToTarget.y, -dirToTarget.x, 0);

        // dirModifier가 1이면 오른쪽에서 왼쪽으로, -1이면 왼쪽에서 오른쪽으로 긁음
        Vector3 sweepStart = targetPos + (rightTangent * dirModifier * sweepWidth) + (Vector3.up * 1.5f); // 긁기 전 높이 치켜듦
        Vector3 sweepEnd = targetPos - (rightTangent * dirModifier * sweepWidth) - (Vector3.up * 0.5f);   // 바닥을 긁으며 내려감

        // 1. 스와이프 시작 위치로 이동 (사전 준비 동작)
        float t = 0;
        while (t < 1f) { t += Time.deltaTime * (stepSpeed * 1.5f); currentPosition = Vector3.Lerp(startPos, sweepStart, t); yield return null; }

        yield return new WaitForSeconds(0.15f); // 살짝 멈칫 (긴장감)

        // 2. 와이퍼처럼 반대편으로 빠르게 긁기 (부채꼴)
        t = 0;
        while (t < 1f) { t += Time.deltaTime * (stepSpeed * 3f); 
            Vector3 linearPos = Vector3.Lerp(sweepStart, sweepEnd, t);
            Vector3 arcOffset = dirToTarget * Mathf.Sin(t * Mathf.PI) * sweepForward; // 앞으로 볼록하게 튀어나오며 부채꼴 궤적 생성
            currentPosition = linearPos + arcOffset; yield return null; }

        yield return new WaitForSeconds(0.5f);
        isStepping = false;
    }

    // [추가] 다리의 트리거(충돌체)에 무언가 닿았을 때 실행되는 유니티 기본 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && bossStats != null && bossStats.enemyData != null)
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats == null) playerStats = collision.GetComponentInParent<PlayerStats>();
            if (playerStats == null) playerStats = collision.transform.root.GetComponentInChildren<PlayerStats>();

            if (playerStats != null && bossStats.enemyData.damage > 0)
            {
                playerStats.TakeDamage(bossStats.enemyData.damage);
            }
        }
    }
}