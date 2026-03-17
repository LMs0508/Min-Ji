using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.Player;

public class StatusUIManager : MonoBehaviour
{
    [Header("Player Reference")]
    private PlayerStats playerStats;
    private PlayerElement playerElement;
    private WeaponManager weaponManager;
    private SkillSlotsPrefab skillSlots;

    [Header("Stat Texts")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI magicText;
    public TextMeshProUGUI defText;

    [Header("Weapon & Element UI")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponDesc;
    public Image elementIcon;
    public TextMeshProUGUI elementDesc;

    void OnEnable()
    {
        // UI가 켜질 때마다 최신 정보로 갱신
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerStats == null) FindPlayer();
        if (playerStats == null) return;

        // 1. 스탯 갱신
        attackText.text = $"ATK: {playerStats.Attack.Value:F0}";
        magicText.text = $"MAG: {playerStats.Magic.Value:F0}";
        defText.text = $"DEF: {playerStats.Defense.Value:F0}";

        // 2. 무기 정보 갱신
        var currentWeapon = weaponManager.currentWeapon;
        if (currentWeapon != null)
        {
            weaponIcon.sprite = currentWeapon.icon;
            weaponIcon.color = Color.white;
            weaponDesc.text = $"{currentWeapon.itemName}\n공격력 +{currentWeapon.attackDamage}";
        }
        else
        {
            weaponIcon.color = new Color(1, 1, 1, 0);
            weaponDesc.text = "무기 없음";
        }

        // 3. 원소 정보 갱신
        elementIcon.sprite = GetElementSprite(playerElement.CurrentElement);
        elementDesc.text = $"{playerElement.CurrentElement} 원소 적용 중";
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            playerElement = player.GetComponent<PlayerElement>();
            weaponManager = player.GetComponent<WeaponManager>();
            skillSlots = player.GetComponent<SkillSlotsPrefab>();
        }
    }

    private Sprite GetElementSprite(Game.Core.ElementType type) 
    {
        // 원소 타입별 스프라이트 반환 로직 추가
        return null; 
    }
}