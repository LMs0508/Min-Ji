using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SlotType { Weapon, Skill }
    public SlotType slotType;
    public int skillIndex = -1; // 스킬일 경우 0~3

    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private RectTransform rect;
    private Canvas canvas;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rect.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        // UI 영역 밖으로 드래그해서 놓았을 때 버리기 실행
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            DropLogic();
        }

        rect.anchoredPosition = originalPosition;
    }

    private void DropLogic()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (slotType == SlotType.Weapon)
        {
            // WeaponManager를 통해 무기 해제 및 드롭 (이전 코드 활용)
            player.GetComponent<WeaponManager>()?.EquipWeapon(null); 
        }
        else if (slotType == SlotType.Skill)
        {
            // SkillSlotsPrefab을 통해 스킬 해제 및 드롭 (SkillSlotsPrefab.cs의 Equip(null, ...) 활용)
            player.GetComponent<SkillSlotsPrefab>()?.Equip(null, null, skillIndex);
        }
    }
}