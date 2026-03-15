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
            // 현재 최종 공격력(기본값 + 보너스)을 반환합니다.
            return stats.Attack.Value;
        }
        return 0;
    }

    public float GetCurrentPlayerMagic()
    {
        if (stats != null && stats.Magic != null)
        {
            // 보너스 마력 20이 포함된 최종 Value를 반환합니다.
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

<<<<<<< Updated upstream
    // A키 입력 시 호출될 함수
    public void OnAttack(Vector2 dir)
    {
        if (equippedWeaponInstance != null)
        {
            equippedWeaponInstance.ExecuteAttack(dir);
=======
    // =========================================================
    // A키 입력 시 호출될 함수 (전투 태세 돌입 추가)
    // =========================================================
    public void OnAttack(Vector2 dir, float multiplier)
    {
        if (equippedWeaponInstance != null)
        {
            // [핵심 추가] 공격할 때 즉시 '전투 태세(Combat Mode)'로 돌입합니다!
            PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
            if (visualHandler != null)
            {
                visualHandler.TriggerCombatMode();
            }

            equippedWeaponInstance.ExecuteAttack(dir, multiplier);
>>>>>>> Stashed changes
        }
        else
        {
            Debug.LogWarning("장착된 무기 프리팹이 없어 공격할 수 없습니다.");
        }
    }

    public void TogglePlayerVisuals(bool isVisible)
    {
        // 1. PlayerVisualHandler 업데이트 원천 차단
        PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.isVisualLocked = !isVisible;
        }

        SpriteRenderer[] srs = transform.root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in srs)
        {
            string objName = sr.gameObject.name;

            if (objName == "Shadow" || objName.Contains("DamageText") || objName.Contains("Die"))
                continue;

            if (equippedWeaponInstance != null && sr.transform.IsChildOf(equippedWeaponInstance.transform))
                continue;

            if (visualHandler != null && visualHandler.WeaponHolder != null)
            {
                if (sr.transform.IsChildOf(visualHandler.WeaponHolder))
                    continue; // 켜지도 끄지도 않고 무시하고 넘어갑니다.
            }
            sr.enabled = isVisible;
        }

        // 3. 플레이어 본체 애니메이터 끄기 (좀비 현상 완벽 방어)
        Animator[] anims = transform.root.GetComponentsInChildren<Animator>(true);
        foreach (Animator anim in anims)
        {
            string objName = anim.gameObject.name;

            if (objName.Contains("Die")) continue;

            if (equippedWeaponInstance != null && anim.transform.IsChildOf(equippedWeaponInstance.transform))
                continue;

            anim.enabled = isVisible;
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