using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // 클릭 감지를 위해 추가

public class QuickSlotUI : MonoBehaviour, IPointerClickHandler // 인터페이스 추가
{
    public int slotNumber;
    public Image icon;
    public TextMeshProUGUI countText;
    public KeyCode hotkey;

    private ItemData assignedItem;

    // 마우스 클릭 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        // 우클릭(Right Click)이 감지되면 슬롯 비우기
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ClearSlot();
            Debug.Log($"{slotNumber}번 단축키 슬롯이 비워졌습니다.");
        }
    }

    public void SetQuickSlot(ItemData item)
    {
        assignedItem = item;
        icon.sprite = item.icon;
        icon.enabled = true;
        UpdateCount();
    }

    private void Update()
    {
        if (Input.GetKeyDown(hotkey))
        {
            UseAssignedItem();
        }

        if (assignedItem != null) UpdateCount();
    }

    private void UpdateCount()
    {
        int totalCount = InventoryManager.Instance.GetItemTotalCount(assignedItem);
        if (totalCount <= 0)
        {
            ClearSlot();
        }
        else
        {
            countText.text = totalCount.ToString();
        }
    }

    private void UseAssignedItem()
    {
        if (assignedItem == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponentInChildren<Game.Player.PlayerStats>();
        if (stats == null) return;

        // 버프 중첩 방지 로직이 포함된 PlayerStats의 함수들 호출
        switch (assignedItem.consumableType)
        {
            case ConsumableType.Health: stats.Heal(assignedItem.value); break;
            case ConsumableType.Mana: stats.RestoreMana(assignedItem.value); break;
            case ConsumableType.SpeedBoost: stats.ApplySpeedBuff(assignedItem.value, assignedItem.duration); break;
            case ConsumableType.AttackBuff: stats.ApplyAttackBuff(assignedItem.value, assignedItem.duration); break;
        }

        InventoryManager.Instance.RemoveItem(assignedItem, 1);
    }

    public void ClearSlot()
    {
        assignedItem = null;
        icon.enabled = false;
        countText.text = "";
    }
}