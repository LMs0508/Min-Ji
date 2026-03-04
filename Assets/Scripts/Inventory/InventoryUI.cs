using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; // 인벤토리 전체 부모 오브젝트
    public Transform slotParent;      // 슬롯들이 배치될 Grid 오브젝트
    public GameObject slotPrefab;     // 슬롯 프리팹

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            if (inventoryPanel.activeSelf) UpdateUI();
        }
    }

    public void UpdateUI()
    {
        // 기존 슬롯 UI들을 모두 삭제 (단순 구현용)
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // 현재 인벤토리 데이터를 바탕으로 슬롯 생성 (소비템 예시)
        foreach (var slotData in InventoryManager.Instance.consumableSlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotParent);
            newSlot.GetComponent<InventorySlotUI>().SetSlot(slotData);
        }

        // 퀘스트 아이템도 필요하다면 아래에 추가...
    }
}