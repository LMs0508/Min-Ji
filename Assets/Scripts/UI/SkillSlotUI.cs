using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Image icon;
    public Image cooldownOverlay;

    SkillData currentSkill;
    float cooldownTimer;

    public void SetSkill(SkillData skill)
    {
        currentSkill = skill;

        if (skill == null)
        {
            icon.enabled = false;
            return;
        }

        icon.enabled = true;
        icon.sprite = skill.icon;
    }

    public void StartCooldown(float duration)
    {
        cooldownTimer = duration;
        cooldownOverlay.fillAmount = 1f;
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownOverlay.fillAmount = cooldownTimer / currentSkill.cooldown;
        }
    }
}