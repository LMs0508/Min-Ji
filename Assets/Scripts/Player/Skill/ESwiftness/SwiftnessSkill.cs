using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // FirstOrDefault 사용을 위해 추가
using Cainos.PixelArtTopDown_Basic;
using Game.Player;
using Game.Core;

public class SwiftnessSkill : MonoBehaviour, ISkill
{
    [Header("UI")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("신속화 설정")]
    public float speedMultiplier = 3f;
    public float duration = 3f;
    public float cooldown = 8f;
    public float skillManaCost = 10f;

    [Header("시각 효과(플레이어 자식 오브젝트 이름)")]
    public string auraChildName = "AuraEffect";

    private float lastUsedTime = -999f;

    public float Cooldown => cooldown;
    public float CooldownRemaining
    {
        get
        {
            float remain = (lastUsedTime + cooldown) - Time.time;
            return Mathf.Max(0f, remain);
        }
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null) return false;

        if (Time.time < lastUsedTime + cooldown)
        {
            Debug.Log("쿨타임 중입니다.");
            return false;
        }

        var stats = owner.GetComponentInChildren<PlayerStats>();
        if (stats == null || !stats.SpendMP(skillManaCost))
        {
            Debug.Log("마나가 부족하거나 PlayerStats를 찾을 수 없습니다.");
            return false;
        }

        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null)
        {
            Debug.LogWarning("SwiftnessSkill: owner에 CoroutineRunner가 없습니다.");
            return false;
        }

        lastUsedTime = Time.time;
        runner.StartCoroutine(SwiftnessRoutine(owner, stats));
        return true;
    }

    private IEnumerator SwiftnessRoutine(GameObject owner, PlayerStats stats)
    {

        
        // 1. 현재 플레이어의 원소 정보 가져오기
        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;

        Debug.Log($"[디버그] 현재 플레이어 원소: {currentElement}");

        // 2. 이 스킬 프리팹에 붙어있는 강화기들 중 현재 원소와 맞는 것 찾기
        var enhancers = GetComponents<ISkillElementEnhancer>();
        Debug.Log($"[디버그] 스킬에 붙은 강화기 총 개수: {enhancers.Length}");

        ISkillElementEnhancer activeEnhancer = enhancers.FirstOrDefault(e => e.TargetElement == currentElement);
        if (activeEnhancer != null)
            Debug.Log($"[디버그] {currentElement} 강화기 찾음! 실행합니다.");
        else
            Debug.LogWarning($"[디버그] {currentElement}에 맞는 강화기를 찾지 못했습니다.");



        // [원소 효과 시작]
        activeEnhancer?.OnStart(owner);

        // 기본 효과: 오라 켜기 및 이동속도 증가
        GameObject aura = FindChildByName(owner.transform, auraChildName);
        if (aura != null) aura.SetActive(true);
        stats.MoveSpeed.Multiply(speedMultiplier);

        float elapsed = 0;
        while (elapsed < duration)
        {
            // [원소 효과 업데이트] (매 프레임 불길 생성 등)
            activeEnhancer?.OnUpdate(owner);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 기본 효과 원복
        stats.MoveSpeed.Divide(speedMultiplier);
        if (aura != null) aura.SetActive(false);

        // [원소 효과 종료]
        activeEnhancer?.OnEnd(owner);
    }

    private GameObject FindChildByName(Transform root, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }
}