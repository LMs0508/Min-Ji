using UnityEngine;
using Game.Player;

public class WeaponManager : MonoBehaviour
{
    [Header("현재 장착 정보")]
    public WeaponData currentWeapon;

    [Header("참조 설정")]
    public Transform weaponHoldPoint; // 플레이어 손 위치 (Transform)

    private WeaponBase equippedWeaponInstance; // 실제 소환된 무기 스크립트
    private PlayerStats stats;

    private void Awake()
    {
        // 부모나 자식 어디에 있든 PlayerStats를 찾습니다.
        stats = GetComponentInParent<PlayerStats>();
        if (stats == null) stats = GetComponentInChildren<PlayerStats>();
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || stats == null)
        {
            Debug.LogError("WeaponData 또는 PlayerStats를 찾을 수 없습니다!");
            return;
        }

        // 1. 기존 무기 제거 (바닥 드롭 + 스탯 원복 + 오브젝트 파괴)
        if (currentWeapon != null)
        {
            DropCurrentWeapon();
            ApplyWeaponStats(currentWeapon, false);

            if (equippedWeaponInstance != null)
            {
                Destroy(equippedWeaponInstance.gameObject);
                equippedWeaponInstance = null;
            }
        }

        // 2. 데이터 할당 및 스탯 적용
        currentWeapon = newWeapon;
        ApplyWeaponStats(currentWeapon, true);

        // 3. 무기 프리팹 소환 (비주얼 및 로직 담당)
        if (currentWeapon.prefab != null && weaponHoldPoint != null)
        {
            GameObject go = Instantiate(currentWeapon.prefab, weaponHoldPoint);
            // 소환된 프리팹에서 무기 로직 스크립트를 가져옵니다.
            equippedWeaponInstance = go.GetComponent<WeaponBase>();

            // 소환된 무기에 데이터를 주입해줍니다.
            if (equippedWeaponInstance != null)
            {
                equippedWeaponInstance.data = currentWeapon;
            }
        }

        Debug.Log($"<color=yellow>{newWeapon.name}</color> 장착 및 프리팹 소환 완료!");
    }

    // A키 입력 시 호출될 함수
    public void OnAttack(Vector2 dir)
    {
        if (equippedWeaponInstance != null)
        {
            equippedWeaponInstance.ExecuteAttack(dir);
        }
        else
        {
            Debug.LogWarning("장착된 무기 프리팹이 없어 공격할 수 없습니다.");
        }
    }

    private void DropCurrentWeapon()
    {
        if (currentWeapon == null || currentWeapon.prefab == null) return;

        // 플레이어 발치에 아이템 드롭
        Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), -0.5f, 0);
        GameObject droppedItem = Instantiate(currentWeapon.prefab, dropPos, Quaternion.identity);

        // 중요: 드롭된 물체는 '발사' 로직이 아닌 '줍기' 로직이 활성화되어야 합니다.
        // 프리팹에 ItemPickup이 붙어있어야 합니다.
        var pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup != null) pickup.itemData = currentWeapon;
    }

    private void ApplyWeaponStats(WeaponData data, bool isEquip)
    {
        if (stats == null) return;

        if (isEquip)
        {
            stats.Attack.AddBonus(data.attackDamage);
            stats.AttackSpeed.Multiply(data.attackSpeedMultiplier);
            stats.Defense.Multiply(data.armorStats);
            stats.CooldownReduction.AddBonus(data.cooldownStats);
            stats.HPRegen.AddBonus(data.hpRegen);
            stats.MPRegen.AddBonus(data.mpRegen);
            stats.MoveSpeed.Multiply(data.playerSpeed);
        }
        else
        {
            stats.Attack.RemoveBonus(data.attackDamage);
            stats.AttackSpeed.Divide(data.attackSpeedMultiplier);
            stats.Defense.Divide(data.armorStats);
            stats.CooldownReduction.RemoveBonus(data.cooldownStats);
            stats.HPRegen.RemoveBonus(data.hpRegen);
            stats.MPRegen.RemoveBonus(data.mpRegen);
            stats.MoveSpeed.Divide(data.playerSpeed);
        }
    }
}