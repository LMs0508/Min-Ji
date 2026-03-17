using Game.Core;
using UnityEngine;

public enum SkillSlot { Q, W, E, R }
public enum DamageType { Attack, Magic }
public enum FullStackEffect { None, DamageBoost, Cleanse, StatBuff }

[CreateAssetMenu(menuName = "Skills/Advanced Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    public Sprite icon;
    [TextArea(3, 10)] public string description;
    public float cooldown = 1f;

    [Header("스킬 성격 (중복 선택 가능)")]
    public bool isActive;   // 공격/액션 기능 여부
    public bool isBuff;     // 스탯 변화 기능 여부
    public bool useCharge;  // 차징 매커니즘 사용 여부
    public bool useStack;   // 스택 매커니즘 사용 여부

    [Header("▼ 속성 시스템 설정")]
    public bool isElementReactive; // 속성에 반응하는 스킬인가?
    public ElementType defaultElement; // 기본 속성

    [Header("속성별 특수 효과 프리팹 (Enhancers)")]
    // 각 속성일 때 실행될 Enhancer(DashFireEnhancer 등)를 연결
    public GameObject fireEnhancerPrefab;
    public GameObject waterEnhancerPrefab;
    public GameObject earthEnhancerPrefab;
    public GameObject windEnhancerPrefab;

    [Header("원소별 추가 설명")]
    [TextArea(3, 10)] public string fireElementDescription;
    [TextArea(3, 10)] public string waterElementDescription;
    [TextArea(3, 10)] public string earthElementDescription;
    [TextArea(3, 10)] public string windElementDescription;
    [TextArea(3, 10)] public string noneElementDescription;


    [Header("공격/전투 계수 (isActive가 체크됐을 때)")]
    public DamageType damageType;
    [Range(0, 5)] public float damageRatio = 1.0f;

    [Header("버프 설정 (isBuff가 체크됐을 때)")]
    public float buffDuration = 5f;
    public float speedMultiplier = 1f;
    public float attackSpeedBoost = 0f;
    public float attackBuffMultiplier = 1f; // 공격력 증가 비율
    public float armor = 1.2f;
    public float cooltimeReduce = 1f;

    [Header("차징 단계별 설정 (useCharge가 체크됐을 때)")]
    [Range(2, 4)] public int maxChargeStages = 3;
    public float[] stageTimeThresholds = { 0.5f, 1.5f, 2.5f };
    public float[] stageMultipliers = { 1f, 2f, 3f };
    public float[] stageRanges = { 1f, 2f, 3f };

    [Header("스택 설정 (useStack이 체크됐을 때)")]
    public int maxStacks = 3;
    public FullStackEffect stackEffect;
    public float stackBonusDamage = 0.5f;

    [Header("시각 효과")]
    public GameObject skillPrefab;

    public string GetElementDescription(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return fireElementDescription;
            case ElementType.Water: return waterElementDescription;
            case ElementType.Earth: return earthElementDescription;
            case ElementType.Wind: return windElementDescription;
            default: return noneElementDescription;
        }
    }
}