using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Player;

public class PlayerDebuffManager : MonoBehaviour
{
    private PlayerStats stats;
    private Animator anim;
    private Rigidbody2D rb;
    private Coroutine burnCoroutine;
    private Coroutine blindFadeCoroutine;

    [Header("UI 설정")]
    public Transform debuffContainer;   // 아이콘들이 배치될 부모 (Horizontal Layout Group)
    public GameObject debuffIconPrefab; // DebuffIconUI 스크립트가 붙은 프리팹
    public List<DebuffIconData> debuffConfigs; // 에디터에서 타입별 아이콘 설정

    [Header("Blind UI")]
    public GameObject blindPanel;

    // 각 디버프별 현재 적용 중인 배율/강도를 저장
    private Dictionary<DebuffType, float> activeDebuffs = new Dictionary<DebuffType, float>();
    // 현재 화면에 떠있는 UI 아이콘들을 관리
    private Dictionary<DebuffType, DebuffIconUI> activeIconUIs = new Dictionary<DebuffType, DebuffIconUI>();

    [System.Serializable]
    public struct DebuffIconData
    {
        public DebuffType type;
        public Sprite iconSprite;
    }

    void Awake()
    {
        stats = GetComponentInChildren<PlayerStats>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyDebuff(DebuffType type, float power, float duration)
    {
        // 중복 디버프 처리 로직
        if (activeDebuffs.ContainsKey(type))
        {
            if (type == DebuffType.Slow && power > activeDebuffs[type]) return;
            if (type != DebuffType.Slow && power < activeDebuffs[type]) return;
            
            // 기존에 같은 디버프가 있다면 UI와 코루틴을 정리하고 새로 시작 (시간 갱신 효과)
            StopExistingDebuff(type);
        }

        StartCoroutine(DebuffRoutine(type, power, duration));
    }

    private void StopExistingDebuff(DebuffType type)
    {
        // 특정 타입의 디버프 UI 제거
        if (activeIconUIs.TryGetValue(type, out DebuffIconUI ui))
        {
            if (ui != null) Destroy(ui.gameObject);
            activeIconUIs.Remove(type);
        }
        // 기존 코루틴은 동일 타입이 들어올 때 ApplyDebuff 레벨에서 정리되거나 
        // 로직에 따라 StopCoroutine을 타입별로 관리할 수 있습니다.
    }

    private IEnumerator DebuffRoutine(DebuffType type, float power, float duration)
    {
        activeDebuffs[type] = power;
        ApplyEffect(type, power, true);

        // --- UI 아이콘 생성 ---
        Sprite icon = debuffConfigs.Find(x => x.type == type).iconSprite;
        if (debuffIconPrefab != null && debuffContainer != null && icon != null)
        {
            GameObject go = Instantiate(debuffIconPrefab, debuffContainer);
            DebuffIconUI ui = go.GetComponent<DebuffIconUI>();
            ui.Setup(icon, duration);
            activeIconUIs[type] = ui;
        }

        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            
            // UI에 남은 시간 전달
            if (activeIconUIs.ContainsKey(type) && activeIconUIs[type] != null)
            {
                activeIconUIs[type].UpdateTime(timer);
            }
            
            yield return null;
        }

        ApplyEffect(type, power, false);
        activeDebuffs.Remove(type);

        // --- UI 아이콘 제거 ---
        if (activeIconUIs.TryGetValue(type, out DebuffIconUI remainingUI))
        {
            if (remainingUI != null) Destroy(remainingUI.gameObject);
            activeIconUIs.Remove(type);
        }
    }

    private void ApplyEffect(DebuffType type, float power, bool isApply)
    {
        switch (type)
        {
            case DebuffType.Slow:
                if (isApply) stats.MoveSpeed.Multiply(power);
                else stats.MoveSpeed.Divide(power);
                break;
            case DebuffType.Weakness:
                if (isApply) stats.Attack.Multiply(power);
                else stats.Attack.Divide(power);
                break;
            case DebuffType.Stun:
                HandleStun(isApply);
                break;
            case DebuffType.Burn:
                if (isApply)
                {
                    if (burnCoroutine != null) StopCoroutine(burnCoroutine);
                    burnCoroutine = StartCoroutine(BurnDamage(power));
                }
                else if (burnCoroutine != null) StopCoroutine(burnCoroutine);
                break;
            case DebuffType.Blind:
                HandleBlind(isApply);
                break;
        }
    }

    private void HandleStun(bool isApply)
    {
        if (isApply)
        {
            stats.MoveSpeed.Multiply(0.0001f);
            if (anim != null) anim.speed = 0f;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            stats.MoveSpeed.Divide(0.0001f);
            if (anim != null) anim.speed = 1f;
        }
    }

    private void HandleBlind(bool isApply)
    {
        if (blindPanel == null) return;
        if (blindFadeCoroutine != null) StopCoroutine(blindFadeCoroutine);
        blindFadeCoroutine = StartCoroutine(FadeBlind(isApply ? 0.8f : 0f));
    }

    private IEnumerator FadeBlind(float targetAlpha)
    {
        Image img = blindPanel.GetComponent<Image>();
        blindPanel.SetActive(true);
        Color color = img.color;
        float startAlpha = color.a;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            img.color = color;
            yield return null;
        }
        if (targetAlpha <= 0f) blindPanel.SetActive(false);
    }

    private IEnumerator BurnDamage(float damage)
    {
        while (true)
        {
            stats.TakeDamage(damage);
            yield return new WaitForSeconds(1f);
        }
    }

    public void ResetAllDebuffs()
    {
        StopAllCoroutines();
        activeDebuffs.Clear();

        // UI 모두 제거
        foreach (var ui in activeIconUIs.Values)
        {
            if (ui != null) Destroy(ui.gameObject);
        }
        activeIconUIs.Clear();

        stats.MoveSpeed.Divide(stats.MoveSpeed.Multiplier);
        stats.Attack.Divide(stats.Attack.Multiplier);

        if (blindPanel != null) blindPanel.SetActive(false);
        if (anim != null) anim.speed = 1f;

        Debug.Log("모든 디버프와 UI가 정화되었습니다.");
    }
}