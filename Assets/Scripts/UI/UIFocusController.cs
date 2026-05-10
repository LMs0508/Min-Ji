using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core; // GameManager가 있는 네임스페이스

public class UIFocusController : MonoBehaviour, IPointerDownHandler
{
    // 패널을 클릭(터치)하는 순간 호출됨
    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FocusPanel(gameObject);
        }
    }
}
