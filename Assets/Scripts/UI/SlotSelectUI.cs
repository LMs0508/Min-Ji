using UnityEngine;

public class SlotSelectUI : MonoBehaviour
{
    private SkillSlotsPrefab slots;
    private GameObject pendingSkillPrefab;

    public void Open(SkillSlotsPrefab slotManager, GameObject skillPrefab)
    {
        slots = slotManager;
        pendingSkillPrefab = skillPrefab;
        gameObject.SetActive(true);
    }

    //  버튼 연결용 (인자 없음)
    public void ChooseQ() => ChooseSlot(0);
    public void ChooseW() => ChooseSlot(1);
    public void ChooseE() => ChooseSlot(2);
    public void ChooseR() => ChooseSlot(3);

    private void ChooseSlot(int slotIndex)
    {
        if (slots == null || pendingSkillPrefab == null) return;

        slots.Equip(pendingSkillPrefab, slotIndex);

        pendingSkillPrefab = null;
        gameObject.SetActive(false);
    }

    public void Close()
    {
        pendingSkillPrefab = null;
        gameObject.SetActive(false);
    }
}