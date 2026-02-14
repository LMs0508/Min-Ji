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

        // 아이콘 가져오기: 스킬 프리팹에 SpriteRenderer가 있으면 그 sprite를 아이콘으로 사용
        var sr = skillPrefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            iconImages[slot].sprite = sr.sprite;
            iconImages[slot].enabled = (sr.sprite != null);
        }
        else
        {
            Debug.LogWarning("스킬 프리팹에 SpriteRenderer가 없어서 아이콘을 못 가져왔어!");
        }
    }
}