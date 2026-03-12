using UnityEngine;
using Game.Player;

public class WeaponManager : MonoBehaviour
{
    [Header("현재 장착 정보")]
    public WeaponData currentWeapon;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || stats == null) return;

        // 1. 기존 무기가 있다면 바닥에 버리고 스탯 원복
        if (currentWeapon != null)
        {
            DropCurrentWeapon(); // 추가된 함수
            ApplyWeaponStats(currentWeapon, false);
        }

        // 2. 새 무기 장착 및 스탯 적용
        currentWeapon = newWeapon;
        ApplyWeaponStats(currentWeapon, true);

        Debug.Log($"{newWeapon.itemName}을(를) 장착했습니다!");
    }

    private void DropCurrentWeapon()
    {
        if (currentWeapon == null) return;

        // ItemData에 등록된 prefab(필드 드롭용 오브젝트)을 소환합니다.
        if (currentWeapon.prefab != null)
        {
            // 플레이어 발치에서 조금 떨어진 위치에 생성
            Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), -0.5f, 0);
            GameObject droppedItem = Instantiate(currentWeapon.prefab, dropPos, Quaternion.identity);

            // 새로 생성된 아이템의 데이터를 현재 장착 해제하는 무기 데이터로 설정
            var pickup = droppedItem.GetComponent<ItemPickup>();
            if (pickup != null) pickup.itemData = currentWeapon;
        }
    }

    private void ApplyWeaponStats(WeaponData data, bool isEquip)
    {
        // [중요] 배율(Multiplier) 스탯은 중첩 계산이 꼬이기 쉬우므로 
        // 무기를 해제할 때는 단순히 나누는게 아니라 원복 로직이 중요합니다.

        if (isEquip)
        {
            // 장착 시: 보너스는 더하고, 배율은 곱함
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
            // 해제 시: 정확히 장착했던 수치만큼 다시 뺌/나눔
            // (이 로직은 PlayerStats의 Stat 클래스가 Divide와 Remove를 지원하므로 사용 가능)
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