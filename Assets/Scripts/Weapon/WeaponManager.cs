using UnityEngine;
using Game.Player;
using System; // [복구] Action 이벤트를 사용하기 위해 꼭 필요합니다!

public class WeaponManager : MonoBehaviour
{
    [Header("현재 장착 정보")]
    public WeaponData currentWeapon;

    public event Action OnSkillFireRequest;
    
    // [복구] UI에 무기가 변경되었음을 알리는 이벤트
    public static event Action<WeaponData> OnWeaponChanged; 

    // [추가] 스킬 시스템에서 현재 무기 인스턴스에 접근하고 발사를 제어하기 위한 속성
    public WeaponBase EquippedWeaponInstance => equippedWeaponInstance;
    public bool IsSkillActive { get; set; } = false;

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
            // 최종 마력을 반환합니다.
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

        // =========================================================
        // 등 뒤 무기 교체 로직
        // =========================================================
        PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.ChangeBackWeapon(newWeapon);
        }

        // [복구] UI 스크립트에게 무기가 바뀌었다고 신호를 보냅니다!
        OnWeaponChanged?.Invoke(currentWeapon);

        Debug.Log($"<color=yellow>{newWeapon.name}</color> 장착 및 프리팹 소환 완료!");
    }

    // 블렌드 트리 다중 이벤트 완벽 차단용 플래그
    private bool hasFiredThisAttack = false;

    // 공격을 새로 시작할 때 무기 스크립트에서 호출하여 발사 권한을 충전합니다.
    public void StartNewAttack()
    {
        hasFiredThisAttack = false; 
    }

    // [추가] 스킬 사용 시 스킬 애니메이션 때문에 일반 투사체가 나가는 것을 막기 위해 권한을 즉시 소모합니다.
    public void ConsumeAttackEvent()
    {
        hasFiredThisAttack = true;
    }

    // 애니메이션 이벤트(PlayerAnimationEventRelay)에서 발사 명령을 받을 함수
    public void FireCurrentWeapon()
    {
        // [핵심] 이미 총알을 쐈다면 시간차로 들어오는 불필요한 이벤트는 완벽히 무시합니다.
        if (hasFiredThisAttack) return;
        hasFiredThisAttack = true;

        // [수정] 스킬이 활성화된 상태라면, 일반 공격 대신 스킬 발사 이벤트를 호출합니다.
        if (IsSkillActive)
        {
            OnSkillFireRequest?.Invoke();
            return;
        }

        if (equippedWeaponInstance != null)
        {
            // [핵심] 오직 현재 손에 들고 있는 무기 프리팹에게만 발사(FireBullet) 명령을 보냅니다!
            equippedWeaponInstance.SendMessage("FireBullet", SendMessageOptions.DontRequireReceiver);
        }
    }

    // A키 입력 시 호출될 함수
    public void OnAttack(Vector2 dir, float multiplier)
    {
        // [핵심] 스킬이 활성화된 상태에서는 일반 공격을 시작할 수 없습니다. (중복 방지)
        if (IsSkillActive) return;

        if (equippedWeaponInstance != null)
        {
            // 공격 시 즉시 전투 태세(Combat Mode)를 활성화합니다.
            PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
            if (visualHandler != null)
            {
                visualHandler.TriggerCombatMode();
            }

            equippedWeaponInstance.ExecuteAttack(dir, multiplier);
        }
        else
        {
            Debug.LogWarning("장착된 무기 프리팹이 없어 공격할 수 없습니다.");
        }
    }

    // 공격 애니메이션 시 플레이어 본체를 숨기는 함수
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

            if (objName == "Shadow" || objName.Contains("DamageText") || objName.Contains("Die"))
                continue;

            if (equippedWeaponInstance != null && sr.transform.IsChildOf(equippedWeaponInstance.transform))
                continue;

            // 등 뒤의 무기(WeaponHolder)는 건드리지 않음
            if (visualHandler != null && visualHandler.WeaponHolder != null)
            {
                if (sr.transform.IsChildOf(visualHandler.WeaponHolder))
                    continue;
            }

            sr.enabled = isVisible;
        }

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

        Vector3 dropPos = transform.position + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), -0.5f, 0);
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