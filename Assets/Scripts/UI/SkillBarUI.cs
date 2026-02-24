using UnityEngine;

public class SkillBarUI : MonoBehaviour
{
    public SkillSlotUI slotQ;
    public SkillSlotUI slotW;
    public SkillSlotUI slotE;
    public SkillSlotUI slotR;

    public void UpdateSlot(int index, SkillData skill)
    {
        switch (index)
        {
            case 0: slotQ.SetSkill(skill); break;
            case 1: slotW.SetSkill(skill); break;
            case 2: slotE.SetSkill(skill); break;
            case 3: slotR.SetSkill(skill); break;
        }
    }
}