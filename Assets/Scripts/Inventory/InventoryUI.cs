using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;

    private void Update()
    {
        // 1. I 키로 인벤토리 열고 닫기
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // 2. ESC 키 처리 (인벤토리가 열려있을 때만 작동)
        if (inventoryPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    public void ToggleInventory()
    {
        bool newState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(newState);

        if (newState)
        {
            UpdateUI();
        }
        else
        {
            // 인벤토리를 닫을 때 툴팁도 함께 숨기기
            if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
        }
    }

    private void HandleEscapeKey()
    {
        // [우선순위 1] 만약 아이템 버리기 팝업이 열려 있다면 팝업부터 닫기
        var dropPopup = FindFirstObjectByType<DropPopupUI>(FindObjectsInactive.Include);
        if (dropPopup != null && dropPopup.gameObject.activeSelf)
        {
            dropPopup.Close();
            return; // 팝업만 닫고 인벤토리는 유지
        }

        // [우선순위 2] 팝업이 없다면 인벤토리 닫기
        ToggleInventory();
    }

    public void UpdateUI()
    {
        // 기존 슬롯 삭제
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // 소비템 슬롯 생성
        foreach (var slotData in InventoryManager.Instance.consumableSlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            newSlot.GetComponent<InventorySlotUI>().SetSlot(slotData);
        }
    }
}