using UnityEngine;
using UnityEngine.U2D.Animation;
using Game.Player;

public class BackWeaponVisual : MonoBehaviour
{
    private SpriteResolver resolver;
    private SpriteRenderer sr;

    private void Awake()
    {
        resolver = GetComponent<SpriteResolver>();
        sr = GetComponent<SpriteRenderer>();

        // 게임 시작 시 기본 상태: 무기 없음 처리
        ClearWeapon();
    }

    public void ChangeWeapon(WeaponData weapon)
    {
        if (resolver == null || sr == null) return;

        // 전달받은 무기 데이터가 없으면 무기를 지움
        if (weapon == null)
        {
            ClearWeapon();
            return;
        }

        // [예외 처리] 매직 건틀렛이면 투명하게 처리
        if (weapon.itemName == "Magicguntlet")
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            return;
        }

        // 그 외의 무기일 경우 원래대로 보이게 복구
        Color normal = sr.color;
        normal.a = 1f;
        sr.color = normal;
        sr.enabled = true;

        // 아이템 타입에 맞춰 카테고리 매칭
        string categoryName = "No category";
        switch (weapon.itemType)
        {
            case ItemType.Melee: categoryName = "Melee"; break;
            case ItemType.Ranged: categoryName = "Range"; break;
            case ItemType.Magic: categoryName = "Magic"; break;
        }

        // 스프라이트 변경
        resolver.SetCategoryAndLabel(categoryName, weapon.itemName);
        Debug.Log($"<color=cyan>[BackWeapon] 등 뒤 무기 변경 -> Category: {categoryName}, Label: {weapon.itemName}</color>");
    }

    // 무기 숨기기 (또는 기본 상태)
    public void ClearWeapon()
    {
        if (sr != null)
        {
            // 가장 확실하게 아무것도 안 보이게 하려면 enabled를 끄는 것이 좋습니다.
            sr.enabled = false; 
        }
        
        // 만약 유니티 스프라이트 라이브러리에 "No category"라는 빈 카테고리를 진짜 만들어 두셨다면
        // resolver.SetCategoryAndLabel("No category", "empty"); 처럼 사용하셔도 됩니다.
    }

    // 전투 모드 돌입/해제 시 PlayerVisualHandler가 호출할 함수
    public void SetVisible(bool isVisible)
    {
        if (sr != null)
        {
            // 스프라이트의 알파값이 0(건틀렛)일 때는 강제로 켜도 어차피 안 보이므로 안전합니다.
            sr.enabled = isVisible;
        }
    }
}