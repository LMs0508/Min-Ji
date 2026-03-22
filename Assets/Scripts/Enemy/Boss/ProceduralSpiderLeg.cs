using UnityEngine;
using System.Collections;

public class ProceduralSpiderLeg : MonoBehaviour
{
    [Header("다리 설정")]
    public Transform body; // 거미 몸통
    public Transform idealPoint; // 다리가 땅을 짚어야 하는 이상적인 목표 위치 (몸통 자식으로 빈 오브젝트 생성 후 할당)
    
    public float stepDistance = 1.5f; // 이 거리 이상 멀어지면 발을 뗌
    public float stepSpeed = 10f; // 발을 내딛는 속도
    public float stepHeight = 0.5f; // 발을 들어올리는 높이

    private Vector3 currentPosition; // 현재 발의 위치
    private bool isStepping = false;

    private void Start()
    {
        currentPosition = idealPoint.position;
    }

    private void Update()
    {
        transform.position = currentPosition;

        // 발이 이상적인 위치에서 너무 멀어지면 새로운 걸음(Step)을 시작함
        if (!isStepping && Vector3.Distance(currentPosition, idealPoint.position) > stepDistance)
        {
            StartCoroutine(Step());
        }
    }

    private IEnumerator Step()
    {
        isStepping = true;
        Vector3 startPos = currentPosition;
        Vector3 targetPos = idealPoint.position; // 도착할 지점

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * stepSpeed;
            currentPosition = Vector3.Lerp(startPos, targetPos, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * stepHeight;
            yield return null;
        }
        isStepping = false;
    }
}