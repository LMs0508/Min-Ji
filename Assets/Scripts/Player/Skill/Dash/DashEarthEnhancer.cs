using Game.Core;
using Game.Player;
using UnityEngine;
using System.Collections;

public class DashEarthEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Earth;

    [Header("대지 강화 설정")]
    public int bonusArmorPerStack = 10;
    public int maxStacks = 3;
    public float resetTime = 6f;

    private int currentStacks = 0;
    private Coroutine resetCoroutine;
    private PlayerStats playerStats;

    public void OnStart(GameObject owner)
    {
        if (playerStats == null)
            playerStats = owner.GetComponentInChildren<PlayerStats>();

        if (playerStats == null) return;

        // [핵심 수정] 새로운 스택이 쌓일 때마다 기존 타이머를 완전히 정지시킵니다.
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        // 최대 스택까지만 방어력 추가
        if (currentStacks < maxStacks)
        {
            currentStacks++;
            playerStats.Defense.AddBonus(bonusArmorPerStack);
            Debug.Log($"<color=brown>[대지]</color> 스택 상승! 현재: {currentStacks}스택 (방어력 +{currentStacks * bonusArmorPerStack})");
        }
        else
        {
            Debug.Log($"<color=brown>[대지]</color> 최대 스택 유지 중...");
        }

        // 스택이 쌓인 후, 혹은 최대 스택 상태에서 다시 썼을 때 타이머를 새로 시작합니다.
        resetCoroutine = StartCoroutine(ResetTimer());
    }

    public void OnUpdate(GameObject owner) { }
    public void OnEnd(GameObject owner) { }

    private IEnumerator ResetTimer()
    {
        // 6초 동안 기다립니다.
        yield return new WaitForSeconds(resetTime);

        // 6초 동안 OnStart가 다시 호출되지 않았다면 여기까지 도달합니다.
        if (currentStacks > 0 && playerStats != null)
        {
            float totalBonusToRemove = currentStacks * bonusArmorPerStack;
            playerStats.Defense.RemoveBonus(totalBonusToRemove);

            Debug.Log($"<color=red>[대지]</color> {resetTime}초 경과! 모든 스택({currentStacks})이 한 번에 사라집니다.");

            currentStacks = 0;
            resetCoroutine = null;
        }
    }
}