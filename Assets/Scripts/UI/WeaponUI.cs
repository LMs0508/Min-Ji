using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public Image weaponIconImage;

    private void OnEnable()
    {
        // WeaponManager의 이벤트를 구독하도록 변경
        WeaponManager.OnWeaponChanged += UpdateWeaponIcon;
    }

    private void OnDisable()
    {
        // 구독 해제 대상도 변경
        WeaponManager.OnWeaponChanged -= UpdateWeaponIcon;
    }

    private void UpdateWeaponIcon(WeaponData weapon)
    {
        if (weapon != null && weapon.icon != null)
        {
            weaponIconImage.sprite = weapon.icon;
            weaponIconImage.color = Color.white;
        }
        else
        {
            weaponIconImage.sprite = null;
            weaponIconImage.color = new Color(1, 1, 1, 0);
        }
    }
}