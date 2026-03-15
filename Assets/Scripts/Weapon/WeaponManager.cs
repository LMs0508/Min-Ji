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
        stats = GetComponentInParent<PlayerStats>();
        if (stats == null) stats = GetComponentInChildren<PlayerStats>();
    }

    public float GetCurrentPlayerAttack()
    {
        if (stats != null && stats.Attack != null)
        {
            return stats.Attack.Value;
        }
        return 0;
    }

    public float GetCurrentPlayerMagic()
    {
        if (stats != null && stats.Magic != null)
        {
            return stats.Magic.Value;
        }
        return 0;
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || stats == null)
        {
            Debug.LogError("WeaponData 또는 PlayerStats를 찾을 수 없습니다!");
            return;
        }

        // 1. 기존 무기 제거
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

        // 3. 무기 프리팹 소환
        if (currentWeapon.prefab != null && weaponHoldPoint != null)
        {
            GameObject go = Instantiate(currentWeapon.prefab, weaponHoldPoint);
            equippedWeaponInstance = go.GetComponent<WeaponBase>();

            ItemPickup pickup = go.GetComponent<ItemPickup>();
            if (pickup != null) pickup.enabled = false;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            if (equippedWeaponInstance != null)
            {
                equippedWeaponInstance.data = currentWeapon;
            }
        }

        Debug.Log($"<color=yellow>{newWeapon.name}</color> 장착 및 프리팹 소환 완료!");
    }

    // =========================================================
    // OnAttack: 전투 태세 돌입 및 공격 실행
    // =========================================================
    public void OnAttack(Vector2 dir, float multiplier)
    {
        if (equippedWeaponInstance != null)
        {
            equippedWeaponInstance.ExecuteAttack(dir, multiplier);
        }
        else
        {
            Debug.LogWarning("장착된 무기 프리팹이 없어 공격할 수 없습니다.");
        }
    }

    // =========================================================
    // TogglePlayerVisuals: 공격 애니메이션 시 본체 숨김 (정석 방식)
    // =========================================================
    public void TogglePlayerVisuals(bool isVisible)
    {
        PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.isVisualLocked = !isVisible;
        }

        SpriteRenderer[] srs = transform.root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in srs)
        {
            string objName = sr.gameObject.name;

            // 예외 처리: 그림자, 데미지 텍스트, 사망 연출
            if (objName == "Shadow" || objName.Contains("DamageText") || objName.Contains("Die"))
                continue;

            // 현재 무기 프리팹 내부의 스프라이트는 끄지 않음
            if (equippedWeaponInstance != null && sr.transform.IsChildOf(equippedWeaponInstance.transform))
                continue;

            // [핵심] 등 뒤의 무기(WeaponHolder)는 PlayerVisualHandler가 관리하므로 건드리지 않음
            if (visualHandler != null && visualHandler.WeaponHolder != null)
            {
                if (sr.transform.IsChildOf(visualHandler.WeaponHolder))
                    continue;
            }

            sr.enabled = isVisible;
        }

        // 플레이어 본체 애니메이터 제어 (Idle 좀비 현상 방지)
        Animator[] anims = transform.root.GetComponentsInChildren<Animator>(true);
        foreach (Animator anim in anims)
        {
            if (anim.gameObject.name.Contains("Die")) continue;

            if (equippedWeaponInstance != null && anim.transform.IsChildOf(equippedWeaponInstance.transform))
                continue;

            anim.enabled = isVisible;
        }
    }

    private void DropCurrentWeapon()
    {
        if (currentWeapon == null || currentWeapon.prefab == null) return;

        Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), -0.5f, 0);
        GameObject droppedItem = Instantiate(currentWeapon.prefab, dropPos, Quaternion.identity);

        var pickup = droppedItem.GetComponent<ItemPickup>();
        if (pickup != null) pickup.itemData = currentWeapon;
    }

    private void ApplyWeaponStats(WeaponData data, bool isEquip)
    {
        if (stats == null) return;

        if (isEquip)
        {
            stats.Attack.AddBonus(data.attackDamage);
            stats.Magic.AddBonus(data.magicPower);
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
            stats.Magic.RemoveBonus(data.magicPower);
            stats.AttackSpeed.Divide(data.attackSpeedMultiplier);
            stats.Defense.Divide(data.armorStats);
            stats.CooldownReduction.RemoveBonus(data.cooldownStats);
            stats.HPRegen.RemoveBonus(data.hpRegen);
            stats.MPRegen.RemoveBonus(data.mpRegen);
            stats.MoveSpeed.Divide(data.playerSpeed);
        }
    }
}