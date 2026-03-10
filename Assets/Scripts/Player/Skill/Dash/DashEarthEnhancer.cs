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

    [Header("UI 연결")]
    public SkillGaugeUI dashGaugeUI; // 인스펙터에서 연결

    private int currentStacks = 0;
    private Coroutine resetCoroutine;
    private PlayerStats playerStats;

    public void OnStart(GameObject owner)
    {
        if (playerStats == null)
            playerStats = owner.GetComponentInChildren<PlayerStats>();

        if (playerStats == null) return;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if (currentStacks < maxStacks)
        {
            currentStacks++;
            playerStats.Defense.AddBonus(bonusArmorPerStack);
        }

        resetCoroutine = StartCoroutine(ResetTimer());
    }

    public void OnUpdate(GameObject owner) { }
    public void OnEnd(GameObject owner) { }

    private IEnumerator ResetTimer()
    {
        float elapsed = 0f;
        while (elapsed < resetTime)
        {
            elapsed += Time.deltaTime;
            // 남은 시간을 게이지로 표시 (6초 -> 0초)
            if (dashGaugeUI != null)
                dashGaugeUI.SetGauge(resetTime - elapsed, resetTime);

            yield return null;
        }

        if (currentStacks > 0 && playerStats != null)
        {
            playerStats.Defense.RemoveBonus(currentStacks * bonusArmorPerStack);
            currentStacks = 0;
        }

        if (dashGaugeUI != null) dashGaugeUI.Hide();
        resetCoroutine = null;
    }
}