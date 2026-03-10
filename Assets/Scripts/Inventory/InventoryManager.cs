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

    // 인벤토리 매니저에 추가할 함수
    public void DropItem(ItemData item, int amount = 1)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // 버릴 개수(amount)만큼 반복해서 프리팹을 생성합니다.
        for (int i = 0; i < amount; i++)
        {
            if (item.prefab != null)
            {
                // 모든 아이템이 한곳에 겹치지 않도록 랜덤한 위치 살짝 추가
                Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Vector3 dropPos = player.transform.position + new Vector3(0.5f, 0, 0) + randomOffset;

                GameObject droppedObj = Instantiate(item.prefab, dropPos, Quaternion.identity);

                var pickup = droppedObj.GetComponent<ItemPickup>();
                if (pickup != null) pickup.itemData = item;
            }
        }

        // 인벤토리 데이터에서 해당 수량만큼 삭제
        RemoveItem(item, amount);
    }

    public void SwapSlots(int indexA, int indexB, ItemType type)
    {
        List<InventorySlot> targetList = (type == ItemType.Consumable) ? consumableSlots : questSlots;

        if (indexA < targetList.Count && indexB < targetList.Count)
        {
            InventorySlot temp = targetList[indexA];
            targetList[indexA] = targetList[indexB];
            targetList[indexB] = temp;

            RefreshUI(); // UI 새로고침하여 바뀐 순서 적용
        }
    }

    public void UseItem(ItemData item)
    {
        if (item == null) return;

        // 1. 플레이어 상태 확인
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var stats = player.GetComponentInChildren<Game.Player.PlayerStats>();
        if (stats == null) return;

        // 2. 아이템 효과 적용 (모든 사용처 공통)
        switch (item.consumableType)
        {
            case ConsumableType.Health: stats.Heal(item.value); break;
            case ConsumableType.Mana: stats.RestoreMana(item.value); break;
            case ConsumableType.SpeedBoost: stats.ApplySpeedBuff(item.value, item.duration); break;
            case ConsumableType.AttackBuff: stats.ApplyAttackBuff(item.value, item.duration); break;
        }

        // 3. 퀘스트 시스템 알림 (중앙에서 한 번만 보고!)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ProgressQuest(QuestType.ItemConsume, item.itemName, 1);
        }

        // 4. 아이템 수량 실제 차감
        RemoveItem(item, 1);

        Debug.Log($"중앙 제어: {item.itemName} 사용 및 퀘스트 보고 완료");
    }

    private void RefreshUI()
    {
        var ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null) ui.UpdateUI();

        // [추가] 인벤토리가 변할 때 퀘스트 목록도 실시간으로 다시 계산하도록 함
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.UpdateQuestUI();
        }
    }
}
