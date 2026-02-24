using UnityEngine;
using UnityEngine.UI;

public class SkillHotbarUI : MonoBehaviour
{
    public SkillSlotsPrefab slots;

    public Image[] iconImages = new Image[4];     // Icon
    public Image[] cooldownImages = new Image[4]; // CoolDown

    private void OnEnable()
    {
        if (slots == null) return;
        slots.OnCooldownChanged += HandleCooldown;
        slots.OnEquipped += HandleEquipped; // 네 기존 아이콘 교체 이벤트
    }

    private void OnDisable()
    {
        if (slots == null) return;
        slots.OnCooldownChanged -= HandleCooldown;
        slots.OnEquipped -= HandleEquipped;
    }

    private void HandleCooldown(int slot, float fill)
    {
        if (slot < 0 || slot >= cooldownImages.Length) return;
        var img = cooldownImages[slot];
        if (img == null) return;

        img.fillAmount = fill;

        // 0이면 숨기기(취향)
        img.enabled = fill > 0.001f;
    }

    private void HandleEquipped(int slot, GameObject skillPrefab)
    {
        if (slot < 0 || slot >= iconImages.Length) return;
        if (iconImages[slot] == null) return;

        var skill = skillPrefab.GetComponent<ISkill>();
        if (skill == null || skill.Icon == null)
        {
            Debug.LogWarning($"스킬 프리팹에 ISkill/Icon이 없어 아이콘을 못 가져왔어! : {skillPrefab.name}");
            iconImages[slot].enabled = false;
            return;
        }

        iconImages[slot].sprite = skill.Icon;
        iconImages[slot].enabled = true;
    }
}