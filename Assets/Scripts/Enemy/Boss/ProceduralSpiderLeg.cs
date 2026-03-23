using UnityEngine;
using System.Collections;

public class ProceduralSpiderLeg : MonoBehaviour
{
    [Header("다리 설정")]
    public Transform body; // 거미 몸통
    
    public float stepDistance = 1.5f; // 이 거리 이상 멀어지면 발을 뗌
    public float stepSpeed = 10f; // 발을 내딛는 속도
    public float stepHeight = 0.5f; // 발을 들어올리는 높이

    private Vector3 currentPosition; // 현재 발의 위치
    private bool isStepping = false;
    private Vector3 defaultLocalPosition; // 시작 시 다리의 자연스러운 로컬 위치 기억

    private void Start()
    {
        // 1. 게임 시작 시, 거미 몸통을 기준으로 '현재 다리가 있는 위치'를 기억해 둡니다.
        defaultLocalPosition = body.InverseTransformPoint(transform.position);

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
}