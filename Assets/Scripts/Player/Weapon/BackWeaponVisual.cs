using UnityEngine;
using UnityEngine.U2D.Animation;
using Game.Player;

public class BackWeaponVisual : MonoBehaviour
{
    private SpriteResolver resolver;
    private SpriteRenderer sr;
    private SpriteLibrary spriteLibrary; // [추가] 스프라이트 라이브러리 직접 확인용

    private void Awake()
    {
        resolver = GetComponent<SpriteResolver>();
        sr = GetComponent<SpriteRenderer>();
        
        // 같은 오브젝트에 있는 SpriteLibrary를 가져옵니다.
        spriteLibrary = GetComponent<SpriteLibrary>();

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
        if (weapon.itemName == "MagicGuntlet")
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

        // =========================================================
        // [핵심] 유니티가 진짜로 이미지를 가지고 있는지 팩트 체크!
        // =========================================================
        if (spriteLibrary != null)
        {
            // Category와 Label이 실제로 라이브러리 안에 존재하는지 검사합니다.
            Sprite checkSprite = spriteLibrary.GetSprite(categoryName, weapon.itemName);
            
            if (checkSprite == null)
            {
                Debug.LogError($"<color=red>[이름 불일치 에러!] Sprite Library 안에 Category: '{categoryName}', Label: '{weapon.itemName}' 이미지가 없습니다! 스크립터블 오브젝트의 itemName과 스펠링/대소문자를 똑같이 맞춰주세요.</color>");
            }
        }
        else
        {
            Debug.LogWarning("WeaponHolder 오브젝트에 'Sprite Library' 컴포넌트가 안 붙어있습니다!");
        }

        // 스프라이트 변경
        resolver.SetCategoryAndLabel(categoryName, weapon.itemName);
        Debug.Log($"<color=cyan>[BackWeapon] 등 뒤 무기 변경 완료 -> Category: {categoryName}, Label: {weapon.itemName}</color>");
    }

    public void ClearWeapon()
    {
        if (sr != null) sr.enabled = false; 
    }

    public void SetVisible(bool isVisible)
    {
        if (sr != null) sr.enabled = isVisible;
    }
}