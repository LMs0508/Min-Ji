using UnityEngine;
using System.Collections;
using Unity.Cinemachine; // 시네머신 네임스페이스 추가

[DefaultExecutionOrder(100)] // 카메라 추적 스크립트보다 무조건 늦게 실행되도록 보장
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private float shakeTimer;
    private float currentIntensity;
    private Vector3 shakeOffset;
    private CinemachineBrain brain;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        brain = GetComponent<CinemachineBrain>();
    }

    public void Shake(float intensity, float duration)
    {
        // 시네머신 중일 때는 기존 쉐이크를 무시하거나 시네머신 임펄스를 써야 함
        if (brain != null && brain.ActiveVirtualCamera != null) return;

        currentIntensity = intensity;
        shakeTimer = duration;
    }

    private void Update()
    {
        // 1. 플레이어 추적 스크립트가 작동하기 전에 이전 프레임에서 더했던 흔들림을 원상복구 (핵심)
        if (shakeOffset != Vector3.zero)
        {
            transform.position -= shakeOffset;
            shakeOffset = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        // 2. 플레이어 추적 스크립트가 카메라 이동을 마친 후(LateUpdate) 새로운 흔들림 오프셋 적용
        if (brain != null && brain.ActiveVirtualCamera != null) return;

        if (shakeTimer > 0)
        {
            shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * currentIntensity;
            transform.position += shakeOffset;
            shakeTimer -= Time.unscaledDeltaTime;
        }
    }
}