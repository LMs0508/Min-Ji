using UnityEngine;

public enum ItemType { Melee, Magic, Ranged, Consumable, Quest, Skill }
public enum ConsumableType { None, Health, Mana, SpeedBoost, AttackBuff }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite icon;
    [TextArea] public string description;

    [Header("중첩 설정")]
    public bool isStackable;   // 중첩 가능 여부
    public int maxStack = 10;  // 최대 중첩 개수

    [Header("공통 기능 설정")]
    [Tooltip("회복량, 공격력, 버프 수치 등 아이템의 핵심 수치를 입력하세요.")]
    public float value;        // 이 하나로 모든 수치를 통합합니다.
    public GameObject prefab;  // 필드 드롭용 프리팹

    [Header("소비/버프 전용 설정")]
    public ConsumableType consumableType; // 효과 종류
    public float duration;                // 지속 시간 (버프가 아닐 경우 0)
}