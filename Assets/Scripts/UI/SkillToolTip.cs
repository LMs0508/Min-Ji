using UnityEngine;
using UnityEngine.EventSystems;
using Game.Player;
using System.Reflection;

public class SkillTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int slotIndex;
    private SkillSlotsPrefab playerSkills;
    private PlayerElement playerElement;

    public void Bind(SkillSlotsPrefab skills, PlayerElement element)
    {
        playerSkills = skills;
        playerElement = element;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playerSkills == null || playerElement == null) return;
        if (slotIndex < 0 || slotIndex >= playerSkills.equippedSkill.Length) return;

        ISkill skill = playerSkills.equippedSkill[slotIndex];
        if (skill == null) return;

        SkillData data = GetSkillDataFromSkill(skill);
        if (data != null && SkillTooltipUI.Instance != null)
        {
            SkillTooltipUI.Instance.Show(data, playerElement.CurrentElement);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SkillTooltipUI.Instance != null) SkillTooltipUI.Instance.Hide();
    }

    private SkillData GetSkillDataFromSkill(ISkill skill)
    {
        if (skill is MonoBehaviour mb)
        {
            // 스킬 스크립트(예: DashFire.cs 등)에 public SkillData skillData; 가 있다고 가정
            FieldInfo field = mb.GetType().GetField("skillData", BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(mb) as SkillData;
        }
        return null;
    }
}