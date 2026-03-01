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


    [Header("Blind UI")]
    public GameObject blindPanel; // 위에서 만든 BlindPanel을 여기에 드래그

    // 각 디버프별 현재 적용 중인 배율/강도를 저장 (가장 강한 것 비교용)
    private Dictionary<DebuffType, float> activeDebuffs = new Dictionary<DebuffType, float>();

    void Awake()
    {
        stats = GetComponentInChildren<PlayerStats>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // 외부(적, 환경)에서 디버프를 걸 때 호출
    public void ApplyDebuff(DebuffType type, float power, float duration)
    {
        // 이미 해당 디버프가 걸려있는데, 새로 걸려는 게 더 약하면 무시
        if (activeDebuffs.ContainsKey(type) && activeDebuffs[type] <= power)
        {
            // (참고: 슬로우의 경우 power가 낮을수록(0.2f) 강한 것이므로 로직에 따라 비교문을 수정)
            if (type == DebuffType.Slow && power > activeDebuffs[type]) return;
            if (type != DebuffType.Slow && power < activeDebuffs[type]) return;
        }

        StartCoroutine(DebuffRoutine(type, power, duration));
    }

    private IEnumerator DebuffRoutine(DebuffType type, float power, float duration)
    {
        activeDebuffs[type] = power;
        ApplyEffect(type, power, true); // 효과 적용

        yield return new WaitForSeconds(duration);

        ApplyEffect(type, power, false); // 효과 제거
        activeDebuffs.Remove(type);
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

            case DebuffType.Stun: // 속박/기절
                HandleStun(isApply);
                break;

            case DebuffType.Silence:
                // 플레이어 컨트롤러의 스킬 사용 bool 변수를 제어하거나 stats에 추가 필요
                Debug.Log(isApply ? "침묵 상태!" : "침묵 해제");
                break;

            case DebuffType.Burn:
                if (isApply)
                {
                    if (burnCoroutine != null) StopCoroutine(burnCoroutine);
                    burnCoroutine = StartCoroutine(BurnDamage(power));
                }
                else
                {
                    if (burnCoroutine != null) StopCoroutine(burnCoroutine);
                }
                break;


            case DebuffType.Blind: // 실명
                HandleBlind(isApply);
                break;
        }
    }
    private void HandleStun(bool isApply)
    {
        if (isApply)
        {
            // multiplier가 0이 되면 복구가 안 되므로 0.0001f를 곱합니다.
            stats.MoveSpeed.Multiply(0.0001f);
            if (anim != null) anim.speed = 0f;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            Debug.Log("캐릭터가 굳었습니다!");
        }
        else
        {
            // 0.0001f를 다시 나눠서 1로 복구합니다.
            stats.MoveSpeed.Divide(0.0001f);
            if (anim != null) anim.speed = 1f;
            Debug.Log("속박 해제!");
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
        float duration = 0.5f; // 0.5초 동안 서서히

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
            yield return new WaitForSeconds(1f); // 1초마다 데미지
        }
    }

    public void ResetAllDebuffs()
    {
        // 1. 모든 실행 중인 디버프 코루틴 정지
        StopAllCoroutines();

        // 2. 딕셔너리 초기화
        activeDebuffs.Clear();

        // 3. 모든 스탯 강제 복구 (기본값으로 세팅)
        stats.MoveSpeed.Divide(stats.MoveSpeed.Multiplier); // 배율을 1로 만듦
        stats.Attack.Divide(stats.Attack.Multiplier);

        // 4. 특수 연출 UI 및 애니메이션 복구
        if (blindPanel != null) blindPanel.SetActive(false);
        if (anim != null) anim.speed = 1f;

        Debug.Log("모든 신체 상태가 정화되었습니다.");
    }
}