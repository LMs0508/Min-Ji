using Game.Core;
using Game.Player; // PlayerStats 접근을 위해 추가
using UnityEngine;

public class JudgementSmashWindEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Wind;
    [Header("바람 강화 설정")]
    public float knockbackMultiplier = 2f;
    private float originalKnockback;

    public void OnStart(GameObject owner)
    {
        var smash = GetComponent<JudgmentSmash>();
        if (smash != null)
        {
            originalKnockback = smash.knockbackForce;
            smash.knockbackForce *= knockbackMultiplier; // 2배 강화
            Debug.Log($"<color=cyan>[바람의 심판]</color> 넉백 강화 적용: {originalKnockback} -> {smash.knockbackForce}");
        }
    }

    public void OnUpdate(GameObject owner)
    {
        // 바람은 매 프레임 실행할 로직이 없다면 비워둡니다.
    }

    public void OnEnd(GameObject owner)
    {
        var smash = GetComponent<JudgmentSmash>();
        if (smash != null)
        {
            smash.knockbackForce = originalKnockback;
            Debug.Log("<color=cyan>[바람의 심판]</color> 넉백 강화 종료");
        }
    }
}