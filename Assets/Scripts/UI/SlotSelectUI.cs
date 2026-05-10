using UnityEngine;

public class SlotSelectUI : MonoBehaviour
{
    private SkillSlotsPrefab slots;
    private GameObject pendingSkillPrefab;
    private GameObject pendingPickupPrefab;

    public void Open(SkillSlotsPrefab slotManager, GameObject skillPrefab, GameObject pickupPrefab)
    {
        slots = slotManager;
        pendingSkillPrefab = skillPrefab;
        pendingPickupPrefab = pickupPrefab;
        gameObject.SetActive(true);
    }


    // 매 프레임 단축키 입력을 확인합니다.
    private void Update()
    {
        // UI가 활성화되어 있을 때만 단축키가 작동하도록 합니다.
        if (!gameObject.activeSelf) return;

        // Ctrl 키(좌우 모두)가 눌려있는지 확인합니다.
        bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isCtrlPressed)
        {
            if (Input.GetKeyDown(KeyCode.Q)) ChooseSlot(0);
            else if (Input.GetKeyDown(KeyCode.W)) ChooseSlot(1);
            else if (Input.GetKeyDown(KeyCode.E)) ChooseSlot(2);
            else if (Input.GetKeyDown(KeyCode.R)) ChooseSlot(3);
        }
    }

    public void ChooseQ() => ChooseSlot(0);
    public void ChooseW() => ChooseSlot(1);
    public void ChooseE() => ChooseSlot(2);
    public void ChooseR() => ChooseSlot(3);

    private void ChooseSlot(int slotIndex)
    {
        if (slots == null || pendingSkillPrefab == null) return;

        // SkillSlotsPrefab에 스킬과 픽업 아이템을 넘겨 장착합니다.
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