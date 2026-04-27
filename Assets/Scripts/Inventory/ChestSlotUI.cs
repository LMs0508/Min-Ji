using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    private ChestItem chestItem;
    private ChestUI owner;

    public void SetItem(ChestItem item, ChestUI chestUI)
    {
        chestItem = item;
        owner = chestUI;

        if (item.itemData != null)
        {
            iconImage.sprite = item.itemData.icon;
            countText.text = item.amount > 1 ? item.amount.ToString() : "";
        }
    }

    public void OnClickTake()
    {
        if (chestItem == null || chestItem.itemData == null) return;

        if (InventoryManager.Instance.AddItem(chestItem.itemData, chestItem.amount))
            owner.OnItemTaken(this, chestItem);
        else
            Debug.Log("인벤토리가 가득 찼습니다.");
    }
}
