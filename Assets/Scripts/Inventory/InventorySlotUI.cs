using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Player;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);

            // 1. 마우스 아래에 무엇이 있는지 확인
            GameObject overGameObject = eventData.pointerCurrentRaycast.gameObject;
            if (overGameObject == null)
            {
                DropItem(); // 빈 공간이면 버리기
                return;
            }

            // 2. 다른 인벤토리 슬롯 위에 놓았을 때 (위치 교체)
            InventorySlotUI targetInventorySlot = overGameObject.GetComponentInParent<InventorySlotUI>();
            if (targetInventorySlot != null && targetInventorySlot != this)
            {
                SwapInventorySlots(targetInventorySlot);
                return;
            }

            // 3. 퀵슬롯 위에 놓았을 때 (등록)
            QuickSlotUI quickSlot = overGameObject.GetComponentInParent<QuickSlotUI>();
            if (quickSlot != null)
            {
                quickSlot.SetQuickSlot(currentItem);
                return;
            }

            // 4. 인벤토리 창 밖(UI가 아예 없는 곳)이면 버리기
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                DropItem();
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1. 아이템이 있고, 2. TooltipUI 인스턴스가 실제로 존재할 때만 실행
        if (currentItem != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.ShowTooltip(currentItem, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TooltipUI가 존재할 때만 끄기 호출
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }


    // 인벤토리 슬롯 끼리 데이터 교체 로직
    private void SwapInventorySlots(InventorySlotUI targetSlot)
    {
        // InventoryManager를 통해 실제 데이터 리스트의 순서를 바꿉니다.
        // 현재는 리스트 기반이므로 인덱스를 찾아 교체하는 함수를 매니저에 구현해야 합니다.
        InventoryManager.Instance.SwapSlots(this.gameObject.transform.GetSiblingIndex(),
                                            targetSlot.gameObject.transform.GetSiblingIndex(),
                                            currentItem.itemType);
    }

    // 마우스가 UI 레이어 위에 있는지 체크하는 보조 함수
    private bool IsPointerOverUI(PointerEventData eventData)
    {
        return eventData.pointerCurrentRaycast.gameObject != null;
    }

    private void DropItem()
    {
        if (currentItem == null) return;

        int totalInInventory = InventoryManager.Instance.GetItemTotalCount(currentItem);

        // 1개 이하면 바로 버리기
        if (totalInInventory <= 1)
        {
            InventoryManager.Instance.DropItem(currentItem, 1);
        }
        // 2개 이상이면 팝업 띄우기
        else
        {
            var popup = FindFirstObjectByType<DropPopupUI>(FindObjectsInactive.Include);
            if (popup != null)
            {
                popup.Open(currentItem, totalInInventory);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
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