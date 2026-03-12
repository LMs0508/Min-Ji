using UnityEngine;

// 무기 종류 구분
public enum WeaponType { Melee, Ranged, MagicDevice }

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData // ItemData를 상속받음
{
    [Header("▼ 무기 기본 스탯")]
    public WeaponType weaponType;
    public float attackDamage = 10f;
    public float attackSpeedMultiplier = 1f; // 1.2 = 20% 증가
    public float armorStats = 1f;
    public float cooldownStats = 0f;
    public float hpRegen = 0f;
    public float mpRegen = 0f;
    public float playerSpeed = 1f;

    [Header("▼ 사거리 및 투사체 설정")]
    [Tooltip("근거리는 공격 판정 거리, 원거리는 투사체 비거리로 사용됩니다.")]
    public float attackRange = 5f;

    [Tooltip("원거리/마도구 무기일 때 발사될 투사체 프리팹")]
    public GameObject projectilePrefab;

    [Tooltip("투사체의 날아가는 속도")]
    public float projectileSpeed = 10f;

    [Header("▼ 추가 옵션")]
    public float magicPower = 0f;    // 마도구용 주문력
    public float manaRegenBonus = 0f; // 마나 재생 추가치
}