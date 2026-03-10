using UnityEngine;
using Game.Core;
using Game.Player;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class WeaponChargeWindEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Wind;

    private WeaponCharge weaponCharge;
    private PlayerStats cachedStats;
    private TopDownCharacterController cachedController;

    public void OnStart(GameObject owner)
    {
        weaponCharge = GetComponent<WeaponCharge>();

        // [수정] 나중에 속도를 올리기 위해 컴포넌트들을 미리 찾아둡니다.
        if (owner != null)
        {
            cachedStats = owner.GetComponentInChildren<PlayerStats>();
            cachedController = owner.GetComponent<TopDownCharacterController>();
            if (cachedController == null) cachedController = owner.GetComponentInChildren<TopDownCharacterController>();
        }
    }

    public void OnUpdate(GameObject owner) { }

    public void OnEnd(GameObject owner) { }

    // 차징 비율에 따른 바람 속성 전용 효과
    public void ApplyWindChargeEffect(float finalRatio)
    {
        if (weaponCharge == null) return;

        float baseWindForce = 10.0f;

        // 1. 넉백 및 경직 수치 설정
        if (finalRatio >= 1.0f)
        {
            weaponCharge.knockbackForce = baseWindForce * 5.0f;
            weaponCharge.stunDuration = 1.0f;
        }
        else if (finalRatio >= 0.5f)
        {
            weaponCharge.knockbackForce = baseWindForce * 2.0f;
            weaponCharge.stunDuration = 1.0f;
        }
        else
        {
            weaponCharge.knockbackForce = baseWindForce;
            weaponCharge.stunDuration = 1.0f;
        }

        // [핵심 수정] 버튼을 뗀 시점(이 함수가 호출되는 시점)에 이동 속도 버프를 실행합니다.
        if (cachedStats != null && cachedController != null)
        {
            StartCoroutine(ApplyWindSpeedBoost(cachedStats, cachedController, 2.0f, 1.5f));
        }

        Debug.Log($"<color=cyan>[바람 속성]</color> 공격 시작! 이속버프 발동 / 넉백:{weaponCharge.knockbackForce}f 적용");
    }

    // 이동 속도 버프 코루틴 (PlayerStats의 Stat 시스템 활용)
    private IEnumerator ApplyWindSpeedBoost(PlayerStats stats, TopDownCharacterController controller, float duration, float multiplier)
    {
        // 1. 배율 적용 (1.5배)
        stats.MoveSpeed.Multiply(multiplier);
        controller.speed = stats.MoveSpeed.Value;

        yield return new WaitForSeconds(duration);

        // 2. 배율 복구
        if (stats != null)
        {
            stats.MoveSpeed.Divide(multiplier);
            if (controller != null)
            {
                controller.speed = stats.MoveSpeed.Value;
            }
        }
    }
}