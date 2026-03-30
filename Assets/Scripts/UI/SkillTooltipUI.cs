using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillTooltipUI : MonoBehaviour
{
    public static SkillTooltipUI Instance;

    [Header("UI 구성 요소")]
    public GameObject root;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI elementEffectText; // 원소 효과 전용 텍스트
    public Image iconImage;

    [Header("설정")]
    public Vector2 offset = new Vector2(15, -15);

    private void Awake()
    {
        Instance = this;
        root.SetActive(false);
    
        // 실행 시 하이어라키 맨 아래로 강제 이동하여 맨 앞에 보이게 함
        transform.SetAsLastSibling();
    }

    public void Show(SkillData data, Game.Core.ElementType currentElement)
    {
        root.SetActive(true);
        nameText.text = data.skillName;
        iconImage.sprite = data.icon;
        
        // 기본 설명
        descriptionText.text = data.description;

        // 원소별 설명 선택 (SkillData의 필드 직접 참조)
        string elementDesc = GetDescriptionByElement(data, currentElement);
        
        if (!string.IsNullOrEmpty(elementDesc))
        {
            elementEffectText.gameObject.SetActive(true);
            elementEffectText.text = $"<color=#FFD700>[원소 반응: {currentElement}]</color>\n{elementDesc}";
        }
        else
        {
            elementEffectText.gameObject.SetActive(false);
        }
    }

    public void Hide() => root.SetActive(false);

    private void Update()
    {
        if (root.activeSelf)
        {
            transform.position = Input.mousePosition + (Vector3)offset;
        }
    }

    private string GetDescriptionByElement(SkillData data, Game.Core.ElementType element)
    {
        return element switch
        {
            Game.Core.ElementType.Fire => data.fireElementDescription,
            Game.Core.ElementType.Water => data.waterElementDescription,
            Game.Core.ElementType.Earth => data.earthElementDescription,
            Game.Core.ElementType.Wind => data.windElementDescription,
            _ => data.noneElementDescription
        };
    }
}