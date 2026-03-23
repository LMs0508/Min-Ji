using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Game.Player;
using System.Reflection; // 리플렉션을 위해 추가

public class SkillTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int slotIndex;
    public GameObject tooltipWindow;
    public TextMeshProUGUI infoText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        var slots = player.GetComponent<SkillSlotsPrefab>();
        var element = player.GetComponent<PlayerElement>();

        // 장착된 스킬 가져오기
        var skill = slots.equippedSkill[slotIndex];
        if (skill == null) return;

        // [수정] 리플렉션으로 SkillData를 찾아서 이름과 설명을 가져옵니다.
        SkillData data = GetSkillData(skill);

        string skillName = data != null ? data.skillName : "스킬 이름 없음";
        string skillDesc = data != null ? data.description : "설명 없음";
        string elementBonus = data != null ? data.GetElementDescription(element.CurrentElement) : "원소 효과 정보 없음";
        
        infoText.text = $"<size=120%>{skillName}</size>\n\n{skillDesc}\n\n<color=cyan>[원소 효과: {element.CurrentElement}]</color>\n{elementBonus}";
        
        tooltipWindow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) => tooltipWindow.SetActive(false);

    private SkillData GetSkillData(ISkill skill)
    {
        // 1. ISkill 구현체에서 SkillData를 찾습니다.
        // (ISkill 인터페이스에 SkillData 프로퍼티가 없으므로 리플렉션으로 'skillData' 또는 'data' 필드를 찾습니다.)
        SkillData data = null;
        var type = skill.GetType();
        
        // 'skillData' 혹은 'data'라는 이름의 필드나 프로퍼티를 찾아서 가져옵니다.
        var field = type.GetField("skillData") ?? type.GetField("data");
        if (field != null && field.FieldType == typeof(SkillData))
            data = field.GetValue(skill) as SkillData;
        
        if (data == null)
        {
            var prop = type.GetProperty("SkillData") ?? type.GetProperty("Data");
            if (prop != null && prop.PropertyType == typeof(SkillData))
                data = prop.GetValue(skill) as SkillData;
        }

        return data;
    }
}