using Game.Core;
using Game.Player; // PlayerStats 접근을 위해 추가
using UnityEngine;

public class SwiftnessWindEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Wind;

    [Tooltip("기본 신속화 속도에 추가로 곱해질 배수입니다. (예: 1.5면 1.5배 더 빨라짐)")]
    public float extraSpeedBonus = 1.5f;

    public void OnStart(GameObject owner)
    {
        // 플레이어의 스탯 컴포넌트 가져오기
        var stats = owner.GetComponentInChildren<PlayerStats>();
        if (stats != null)
        {
            // 바람 속성 추가 가속 적용
            stats.MoveSpeed.Multiply(extraSpeedBonus);
            Debug.Log($"<color=cyan>[바람의 신속화]</color> 추가 가속 {extraSpeedBonus}배 적용!");
        }
    }

    public void OnUpdate(GameObject owner)
    {
        // 바람은 매 프레임 실행할 로직이 없다면 비워둡니다.
    }

    public void OnEnd(GameObject owner)
    {
        var stats = owner.GetComponentInChildren<PlayerStats>();
        if (stats != null)
        {
            // 적용했던 추가 배수만큼 다시 나누어서 원복
            stats.MoveSpeed.Divide(extraSpeedBonus);
            Debug.Log("<color=cyan>[바람의 신속화]</color> 추가 가속 종료");
        }
    }
}