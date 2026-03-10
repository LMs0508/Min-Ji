using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropPopupUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public TextMeshProUGUI titleText;

    private ItemData currentItem;
    private int maxCount;
    private int currentCount = 1;

    public void Open(ItemData item, int totalCount)
    {
        currentItem = item;
        maxCount = totalCount;
        currentCount = 1;

        titleText.text = $"{item.itemName} 버리기";
        gameObject.SetActive(true);
        UpdateDisplay();
    }

    // [추가] 0 버튼: 즉시 최소 수량(1)으로 설정
    public void OnClickZero()
    {
        currentCount = 1;
        UpdateDisplay();
    }

    // [추가] M 버튼: 즉시 최대 보유량으로 설정
    public void OnClickMax()
    {
        currentCount = maxCount;
        UpdateDisplay();
    }

    public void OnClickPlus()
    {
        if (currentCount < maxCount) currentCount++;
        UpdateDisplay();
    }

    public void OnClickMinus()
    {
        if (currentCount > 1) currentCount--;
        UpdateDisplay();
    }

    public void OnInputEndEdit(string value)
    {
        if (int.TryParse(value, out int result))
        {
            currentCount = Mathf.Clamp(result, 1, maxCount);
        }
        else
        {
            currentCount = 1;
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        inputField.text = currentCount.ToString();
    }

    public void OnClickConfirm()
    {
        InventoryManager.Instance.DropItem(currentItem, currentCount);
        Close();
    }

    public void Close() => gameObject.SetActive(false);
}