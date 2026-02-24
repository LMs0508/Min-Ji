using UnityEngine;

public class SlotSelectUI : MonoBehaviour
{
    private SkillSlotsPrefab slots;
    private GameObject pendingSkillPrefab;
    private GameObject pendingPickupPrefab; //  추가

    public void Open(SkillSlotsPrefab slotManager, GameObject skillPrefab, GameObject pickupPrefab)
    {
        slots = slotManager;
        pendingSkillPrefab = skillPrefab;
        pendingPickupPrefab = pickupPrefab;
        gameObject.SetActive(true);
    }

    public void ChooseQ() => ChooseSlot(0);
    public void ChooseW() => ChooseSlot(1);
    public void ChooseE() => ChooseSlot(2);
    public void ChooseR() => ChooseSlot(3);

    private void ChooseSlot(int slotIndex)
    {
        if (slots == null || pendingSkillPrefab == null) return;

        //  이제 pickup도 같이 넘긴다
        slots.Equip(pendingSkillPrefab, pendingPickupPrefab, slotIndex);

        pendingSkillPrefab = null;
        pendingPickupPrefab = null;
        gameObject.SetActive(false);
    }

    public void Close()
    {
        pendingSkillPrefab = null;
        pendingPickupPrefab = null;
        gameObject.SetActive(false);
    }
}