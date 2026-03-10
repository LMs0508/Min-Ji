using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// [수정] IPointerEnterHandler, IPointerExitHandler 인터페이스 추가
public class QuickSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int slotNumber;
    public Image icon;
    public TextMeshProUGUI countText;
    public KeyCode hotkey;

    private ItemData assignedItem;
    private GameObject dragIcon;
    private Canvas mainCanvas;

    private void Awake()
    {
        mainCanvas = GetComponentInParent<Canvas>();
    }

    // --- 마우스 툴팁 로직 추가 ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 아이템이 등록되어 있고, 툴팁 인스턴스가 존재할 때만 표시
        if (assignedItem != null && TooltipUI.Instance != null)
        {
            TooltipUI.Instance.ShowTooltip(assignedItem, transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 툴팁 인스턴스가 존재할 때만 숨김
        if (TooltipUI.Instance != null)
        {
            TooltipUI.Instance.HideTooltip();
        }
    }

    // --- 마우스 우클릭 시 슬롯 비우기 ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ClearSlot();
            // 우클릭으로 비웠으니 툴팁도 즉시 숨겨야 어색하지 않습니다.
            if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();
        }
    }

    // --- 드래그 로직 ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedItem == null) return;

        // 드래그 시작 시 툴팁 숨기기 (잡아당기는데 설명이 떠있으면 방해됨)
        if (TooltipUI.Instance != null) TooltipUI.Instance.HideTooltip();

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(mainCanvas.transform);
        dragIcon.transform.SetAsLastSibling();

        var image = dragIcon.AddComponent<Image>();
        image.sprite = assignedItem.icon;
        image.raycastTarget = false;

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

            GameObject overObj = eventData.pointerCurrentRaycast.gameObject;
            if (overObj != null)
            {
                QuickSlotUI targetQuickSlot = overObj.GetComponentInParent<QuickSlotUI>();

                if (targetQuickSlot != null && targetQuickSlot != this)
                {
                    ItemData tempItem = targetQuickSlot.GetAssignedItem();
                    targetQuickSlot.SetQuickSlot(this.assignedItem);

                    if (tempItem != null) this.SetQuickSlot(tempItem);
                    else this.ClearSlot();
                }
            }
        }
    }

    // --- 아이템 설정 및 사용 로직 ---
    public void SetQuickSlot(ItemData item)
    {
        if (item == null) { ClearSlot(); return; }
        assignedItem = item;
        icon.sprite = item.icon;
        icon.enabled = true;
        UpdateCount();
    }

    private void Update()
    {
        if (Input.GetKeyDown(hotkey)) UseAssignedItem();
        if (assignedItem != null) UpdateCount();
    }

    private void UpdateCount()
    {
        int totalCount = InventoryManager.Instance.GetItemTotalCount(assignedItem);
        if (totalCount <= 0) ClearSlot();
        else countText.text = totalCount.ToString();
    }

    public ItemData GetAssignedItem() => assignedItem;

    private void UseAssignedItem() // 기존의 복잡한 로직 삭제
    {
        if (assignedItem == null) return;

        // 중앙 매니저에게 모든 권한 위임
        InventoryManager.Instance.UseItem(assignedItem);
    }

    public void ClearSlot()
    {
        assignedItem = null;
        icon.enabled = false;
        countText.text = "";
    }
}