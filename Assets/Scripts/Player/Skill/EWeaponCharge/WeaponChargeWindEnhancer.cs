using UnityEngine;
using Game.Core;
using Game.Player; // PlayerStats 접근을 위해 필수 추가
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class WeaponChargeWindEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Wind;

    private WeaponCharge weaponCharge;

    public void OnStart(GameObject owner)
    {
        weaponCharge = GetComponent<WeaponCharge>();

        // [수정] PlayerStats 컴포넌트를 가져옵니다.
        PlayerStats stats = owner.GetComponentInChildren<PlayerStats>();
        TopDownCharacterController controller = owner.GetComponent<TopDownCharacterController>();
        if (controller == null) controller = owner.GetComponentInChildren<TopDownCharacterController>();

        if (stats != null && controller != null)
        {
            // PlayerStats의 Stat 시스템을 사용하여 속도 버프 적용
            StartCoroutine(ApplyWindSpeedBoost(stats, controller, 2.0f, 1.5f));
        }
    }

    public void OnUpdate(GameObject owner) { }

    public void OnEnd(GameObject owner) { }

    // 바람 속성 전용 이속 버프 코루틴
    private IEnumerator ApplyWindSpeedBoost(PlayerStats stats, TopDownCharacterController controller, float duration, float multiplier)
    {
        // 1. Stat 클래스의 Multiply 기능을 사용하여 배율 적용 (1.5배)
        stats.MoveSpeed.Multiply(multiplier);

        // 2. 컨트롤러의 speed를 즉시 업데이트 (동기화)
        controller.speed = stats.MoveSpeed.Value;

        yield return new WaitForSeconds(duration);

        // 3. Stat 클래스의 Divide 기능을 사용하여 배율 원상 복구
        if (stats != null)
        {
            stats.MoveSpeed.Divide(multiplier);

            // 4. 컨트롤러의 speed를 다시 원래대로 업데이트
            if (controller != null)
            {
                controller.speed = stats.MoveSpeed.Value;
            }
        }
    }

    // 차징 비율에 따른 바람 속성 전용 효과 (기존 로직 유지)
    public void ApplyWindChargeEffect(float finalRatio)
    {
        if (weaponCharge == null) return;

        float baseWindForce = 10.0f;

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

        Debug.Log($"<color=cyan>[바람 속성]</color> 이속버프/넉백:{weaponCharge.knockbackForce}f/경직:{weaponCharge.stunDuration}s 적용");
    }
}