using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    private bool isPlayerNearby;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.G))
        {
            HandlePickup();
        }
    }

    private void HandlePickup()
    {
        switch (itemData.itemType)
        {
            case ItemType.Melee:
            case ItemType.Magic:
            case ItemType.Ranged:
                // 스킬이나 무기는 기존처럼 슬롯 선택 UI 오픈
                OpenSlotSelectUI();
                break;

            case ItemType.Consumable:
            case ItemType.Quest:
                // 소비/퀘스트 템은 인벤토리에 즉시 추가
                if (InventoryManager.Instance.AddItem(itemData))
                {
                    Destroy(gameObject);
                }
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // 안내 문구를 띄우고 싶다면 여기에 추가
            Debug.Log("G 키를 눌러 아이템 획득");
        }
    }

    // 플레이어가 범위를 벗어났을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void OpenSlotSelectUI()
    {
        var slotUI = FindFirstObjectByType<SlotSelectUI>();
        var slots = FindFirstObjectByType<SkillSlotsPrefab>();
        // 기존 Equip 로직 호출...
        slotUI.Open(slots, itemData.prefab, itemData.prefab);
        Destroy(gameObject);
    }
}