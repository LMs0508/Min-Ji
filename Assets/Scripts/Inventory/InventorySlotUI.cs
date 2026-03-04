using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Player;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public TextMeshProUGUI countText;
    private ItemData currentItem; // 현재 이 칸에 들어있는 아이템 정보
    private Canvas mainCanvas;
    private GameObject dragIcon; // 드래그할 때 따라다닐 가짜 아이콘

    private void Awake()
    {
        mainCanvas = GetComponentInParent<Canvas>();
    }

    

    public void SetSlot(InventorySlot slot)
    {
        currentItem = slot.item;
        icon.sprite = slot.item.icon;
        icon.enabled = true;

        // 중첩 가능한 아이템이고 1개보다 많을 때만 숫자 표시
        if (slot.item.isStackable && slot.count > 1)
        {
            countText.text = slot.count.ToString();
            countText.gameObject.SetActive(true);
        }
        else
        {
            countText.gameObject.SetActive(false);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.enabled = false;
        countText.gameObject.SetActive(false);
    }

    // 슬롯 클릭 시 아이템 사용
    public void OnClickSlot()
    {
        if (currentItem == null) return;

        // 소비 아이템(포션 등)일 때만 실행
        if (currentItem.itemType == ItemType.Consumable)
        {
            UseItem();
        }
    }

    private void UseItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponentInChildren<PlayerStats>();
        if (stats == null) return;

        // 아이템 종류에 따라 다른 함수 실행
        switch (currentItem.consumableType)
        {
            case ConsumableType.Health:
                stats.Heal(currentItem.value); // value를 회복량으로 사용
                break;

            case ConsumableType.Mana:
                stats.RestoreMana(currentItem.value); // value를 마나 회복량으로 사용
                break;

            case ConsumableType.SpeedBoost:
                // value를 이속 배율로, duration을 시간으로 사용
                stats.ApplySpeedBuff(currentItem.value, currentItem.duration);
                break;
        
            case ConsumableType.AttackBuff:
                stats.ApplyAttackBuff(currentItem.value, currentItem.duration);
                break;
        }

        Debug.Log($"{currentItem.itemName} 사용됨: {currentItem.consumableType} 효과");
        InventoryManager.Instance.RemoveItem(currentItem, 1);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // 드래그용 가짜 아이콘 생성
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(mainCanvas.transform);
        dragIcon.transform.SetAsLastSibling(); // 맨 앞에 보이게

        var image = dragIcon.AddComponent<Image>();
        image.sprite = currentItem.icon;
        image.raycastTarget = false; // 드래그 도중 마우스 이벤트를 방해하지 않게

        dragIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);

            // 마우스 위치에 단축키 슬롯이 있는지 확인
            QuickSlotUI quickSlot = GetQuickSlotUnderMouse();
            if (quickSlot != null)
            {
                quickSlot.SetQuickSlot(currentItem);
            }
        }
    }

    private QuickSlotUI GetQuickSlotUnderMouse()
    {
        // 레이캐스트를 통해 마우스 아래의 QuickSlotUI를 찾습니다.
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            QuickSlotUI slot = result.gameObject.GetComponent<QuickSlotUI>();
            if (slot != null) return slot;
        }
        return null;
    }
}