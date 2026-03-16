using UnityEngine;
using Game.Player;
<<<<<<< Updated upstream
=======
using System;
using UnityEngine.U2D.Animation;
>>>>>>> Stashed changes

public class WeaponManager : MonoBehaviour
{
    [Header("ÇöÀç ÀåÂø Á¤º¸")]
    public WeaponData currentWeapon;

    [Header("ÂüÁ¶ ¼³Á¤")]
    public Transform weaponHoldPoint; // ÇÃ·¹ÀÌ¾î ¼Õ À§Ä¡ (Transform)

    private WeaponBase equippedWeaponInstance; // ½ÇÁ¦ ¼ÒÈ¯µÈ ¹«±â ½ºÅ©¸³Æ®
    private PlayerStats stats;

    private void Awake()
    {
        // ºÎ¸ğ³ª ÀÚ½Ä ¾îµğ¿¡ ÀÖµç PlayerStats¸¦ Ã£½À´Ï´Ù.
        stats = GetComponentInParent<PlayerStats>();
        if (stats == null) stats = GetComponentInChildren<PlayerStats>();
    }

    public float GetCurrentPlayerAttack()
    {
        if (stats != null && stats.Attack != null)
        {
            // ÇöÀç ÃÖÁ¾ °ø°İ·Â(±âº»°ª + º¸³Ê½º)À» ¹İÈ¯ÇÕ´Ï´Ù.
            return stats.Attack.Value;
        }
        return 0;
    }

    public float GetCurrentPlayerMagic()
    {
        if (stats != null && stats.Magic != null)
        {
            // ÃÖÁ¾ ¸¶·ÂÀ» ¹İÈ¯ÇÕ´Ï´Ù.
            return stats.Magic.Value;
        }
        return 0;
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || stats == null)
        {
            Debug.LogError("WeaponData ¶Ç´Â PlayerStats¸¦ Ã£À» ¼ö ¾ø½À´Ï´Ù!");
            return;
        }

        // 1. ±âÁ¸ ¹«±â Á¦°Å (¹Ù´Ú µå·Ó + ½ºÅÈ ¿øº¹ + ¿ÀºêÁ§Æ® ÆÄ±«)
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

        // 2. µ¥ÀÌÅÍ ÇÒ´ç ¹× ½ºÅÈ Àû¿ë
        currentWeapon = newWeapon;
        ApplyWeaponStats(currentWeapon, true);

        // 3. ¹«±â ÇÁ¸®ÆÕ ¼ÒÈ¯ (ºñÁÖ¾ó ¹× ·ÎÁ÷ ´ã´ç)
        if (currentWeapon.prefab != null && weaponHoldPoint != null)
        {
            GameObject go = Instantiate(currentWeapon.prefab, weaponHoldPoint);
            equippedWeaponInstance = go.GetComponent<WeaponBase>();

            // ÀåÂøµÈ ¹«±â ÇÁ¸®ÆÕÀÇ ¾ÆÀÌÅÛ Áİ±â ±â´É°ú ±âº» ÀÌ¹ÌÁö´Â ²ü´Ï´Ù.
            ItemPickup pickup = go.GetComponent<ItemPickup>();
            if (pickup != null) pickup.enabled = false;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            if (equippedWeaponInstance != null)
            {
                equippedWeaponInstance.data = currentWeapon;
            }
        }
<<<<<<< Updated upstream

        Debug.Log($"<color=yellow>{newWeapon.name}</color> ÀåÂø ¹× ÇÁ¸®ÆÕ ¼ÒÈ¯ ¿Ï·á!");
    }

    // AÅ° ÀÔ·Â ½Ã È£ÃâµÉ ÇÔ¼ö
=======
        PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.ChangeBackWeapon(newWeapon);
        }        
        OnWeaponChanged?.Invoke(currentWeapon);  
        Debug.Log($"<color=yellow>{newWeapon.name}</color> ì¥ì°© ë° í”„ë¦¬íŒ¹ ì†Œí™˜ ì™„ë£Œ!");
    }

    private void UpdateBackWeaponSprite(WeaponData weapon)
    {
        PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null && visualHandler.WeaponHolder != null)
        {
            SpriteResolver resolver = visualHandler.WeaponHolder.GetComponent<SpriteResolver>();
            SpriteRenderer sr = visualHandler.WeaponHolder.GetComponent<SpriteRenderer>();

            if (resolver != null && sr != null)
            {
                // [ì˜ˆì™¸ ì²˜ë¦¬] ë¬´ê¸° ì´ë¦„ì´ "Magicguntlet"ì´ë©´ ë“± ë’¤ ìŠ¤í”„ë¼ì´íŠ¸ë¥¼ íˆ¬ëª…í•˜ê²Œ ìˆ¨ê¹ë‹ˆë‹¤.
                if (weapon.itemName == "Magicguntlet")
                {
                    Color c = sr.color;
                    c.a = 0f; 
                    sr.color = c;
                    return; // ì¹´í…Œê³ ë¦¬ ë³€ê²½ì„ í•˜ì§€ ì•Šê³  ë°”ë¡œ ì¢…ë£Œ
                }
                
                // Magicguntletì´ ì•„ë‹ˆë¼ë©´ íˆ¬ëª…ë„ë¥¼ ë‹¤ì‹œ ì›ë˜ëŒ€ë¡œ(100%) ëŒë ¤ë†“ìŠµë‹ˆë‹¤.
                Color normalColor = sr.color;
                normalColor.a = 1f;
                sr.color = normalColor;

                // ItemTypeì„ ê¸°ë°˜ìœ¼ë¡œ Sprite Libraryì˜ Category ì´ë¦„ ê²°ì •
                string categoryName = "";
                switch (weapon.itemType)
                {
                    case ItemType.Melee: categoryName = "Melee"; break;
                    case ItemType.Ranged: categoryName = "Range"; break;
                    case ItemType.Magic: categoryName = "Magic"; break;
                }

                // ë¬´ê¸° ì´ë¦„(itemName)ì„ Label ì´ë¦„ìœ¼ë¡œ ì‚¬ìš©í•˜ì—¬ ìŠ¤í”„ë¼ì´íŠ¸ë¥¼ ë³€ê²½í•©ë‹ˆë‹¤.
                resolver.SetCategoryAndLabel(categoryName, weapon.itemName);
            }
        }
    }
    // Aí‚¤ ì…ë ¥ ì‹œ í˜¸ì¶œë  í•¨ìˆ˜
>>>>>>> Stashed changes
    public void OnAttack(Vector2 dir, float multiplier)
    {
        if (equippedWeaponInstance != null)
        {
            // [ÇÙ½É Ãß°¡] °ø°İ ½Ã Áï½Ã ÀüÅõ ÅÂ¼¼(Combat Mode)¸¦ È°¼ºÈ­ÇÕ´Ï´Ù.
            PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
            if (visualHandler != null)
            {
                visualHandler.TriggerCombatMode();
            }

            equippedWeaponInstance.ExecuteAttack(dir, multiplier);
        }
        else
        {
            Debug.LogWarning("ÀåÂøµÈ ¹«±â ÇÁ¸®ÆÕÀÌ ¾ø¾î °ø°İÇÒ ¼ö ¾ø½À´Ï´Ù.");
        }
    }

    // °ø°İ ¾Ö´Ï¸ŞÀÌ¼Ç ½Ã ÇÃ·¹ÀÌ¾î º»Ã¼¸¦ ¼û±â´Â ÇÔ¼ö
    public void TogglePlayerVisuals(bool isVisible)
    {
        // 1. PlayerVisualHandler ¾÷µ¥ÀÌÆ® ¿øÃµ Â÷´Ü (Á»ºñ Çö»ó ¹æÁö)
        PlayerVisualHandler visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.isVisualLocked = !isVisible;
        }

        // 2. ½ºÇÁ¶óÀÌÆ® ·»´õ·¯ Ã³¸®
        SpriteRenderer[] srs = transform.root.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in srs)
        {
            string objName = sr.gameObject.name;

            // ¿¹¿Ü Ç×¸ñ Ã¼Å©
            if (objName == "Shadow" || objName.Contains("DamageText") || objName.Contains("Die"))
                continue;

            // ÇöÀç ÀåÂøµÈ ¹«±â ÀÌÆåÆ®´Â ²ôÁö ¾Ê½À´Ï´Ù.
            if (equippedWeaponInstance != null && sr.transform.IsChildOf(equippedWeaponInstance.transform))
                continue;

            // [ÇÙ½É ¿¹¿Ü] µî µÚÀÇ ¹«±â(WeaponHolder)´Â PlayerVisualHandler°¡ Á¦¾îÇÏµµ·Ï ³»¹ö·ÁµÓ´Ï´Ù.
            if (visualHandler != null && visualHandler.WeaponHolder != null)
            {
                if (sr.transform.IsChildOf(visualHandler.WeaponHolder))
                    continue;
            }

            sr.enabled = isVisible;
        }

        // 3. º»Ã¼ ¾Ö´Ï¸ŞÀÌÅÍ Á¦¾î (Idle °­Á¦ Àç»ı ¹æÁö)
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