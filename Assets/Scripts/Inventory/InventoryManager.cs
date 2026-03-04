using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventorySlot> consumableSlots = new List<InventorySlot>();
    public List<InventorySlot> questSlots = new List<InventorySlot>();

    public int maxConsumableSlots = 20;

    private void Awake() => Instance = this;

    // 아이템 추가
    public bool AddItem(ItemData item, int amount = 1)
    {
        List<InventorySlot> targetList = (item.itemType == ItemType.Consumable) ? consumableSlots : questSlots;
        int maxSlots = (item.itemType == ItemType.Consumable) ? maxConsumableSlots : 999;

        // 1. 중첩 시도
        if (item.isStackable)
        {
            foreach (var slot in targetList)
            {
                if (slot.item == item && !slot.IsFull)
                {
                    int canAdd = item.maxStack - slot.count;
                    int toAdd = Mathf.Min(canAdd, amount);
                    slot.count += toAdd;
                    amount -= toAdd;
                    if (amount <= 0) { RefreshUI(); return true; }
                }
            }
        }

        // 2. 새 슬롯 추가
        while (amount > 0)
        {
            if (targetList.Count >= maxSlots) return false;
            int toAdd = Mathf.Min(amount, item.maxStack);
            targetList.Add(new InventorySlot(item, toAdd));
            amount -= toAdd;
        }

        RefreshUI();
        return true;
    }

    // 아이템 제거 (수량이 0이 되면 삭제)
    public void RemoveItem(ItemData item, int amount = 1)
    {
        List<InventorySlot> targetList = (item.itemType == ItemType.Consumable) ? consumableSlots : questSlots;

        for (int i = targetList.Count - 1; i >= 0; i--)
        {
            if (targetList[i].item == item)
            {
                if (targetList[i].count > amount)
                {
                    targetList[i].count -= amount;
                    amount = 0;
                }
                else
                {
                    amount -= targetList[i].count;
                    targetList.RemoveAt(i);
                }
                if (amount <= 0) break;
            }
        }
        RefreshUI();
    }

    public int GetItemTotalCount(ItemData item)
    {
        if (item == null) return 0;

        int total = 0;
        // 소비템 리스트와 퀘스트템 리스트 모두에서 해당 아이템을 찾아 합산합니다.
        List<InventorySlot> targetList = (item.itemType == ItemType.Consumable) ? consumableSlots : questSlots;

        foreach (var slot in targetList)
        {
            if (slot.item == item)
            {
                total += slot.count;
            }
        }
        return total;
    }

    private void RefreshUI()
    {
        // UI가 만들어지면 여기서 호출할 예정입니다.
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.UpdateUI();
    }
}
