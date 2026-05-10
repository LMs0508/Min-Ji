using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotParent;
    public GameObject slotPrefab;
    
    [Header("UI Buttons")]
    public Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => 
            {
                // [수정] 스크립트가 패널과 다른 오브젝트에 붙어있을 경우를 대비해 inventoryPanel을 직접 닫습니다.
                if (inventoryPanel != null) inventoryPanel.SetActive(false);
                else gameObject.SetActive(false);
            });
        }
    }

    // 창이 켜질 때마다 UI 갱신
    private void OnEnable()
    {
        UpdateUI();
    }

    // 창이 꺼질 때 툴팁도 같이 숨김
    private void OnDisable()
    {
        if (TooltipUI.Instance != null) 
            TooltipUI.Instance.HideTooltip();
    }

    public void UpdateUI()
    {
        // [수정] 게임 시작 시점 등 InventoryManager가 아직 초기화되지 않았을 때 오류 방지
        if (InventoryManager.Instance == null) return;

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