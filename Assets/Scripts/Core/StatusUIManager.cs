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

    [Header("Stat Texts (TMP)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI magicText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI moveSpeedText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI HPregenText; // HP/MP 재생 통합 혹은 분리
    public TextMeshProUGUI MPregenText; // HP/MP 재생 통합 혹은 분리

    [Header("Weapon & Element UI")]
    public Image weaponIcon;
    public TextMeshProUGUI weaponDesc;
    public Image elementIcon;
    public TextMeshProUGUI elementDesc;

    void OnEnable()
    {
        RefreshUI();
    }

    // [수정] 창이 열려있는 동안 실시간으로 스탯 변화(체력 재생 등)를 반영하기 위해 Update 추가
    void Update()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerStats == null) FindPlayer();
        if (playerStats == null) return;

        // 1. 모든 스탯 갱신 (PlayerStats의 Stat 클래스 Value 사용)
        hpText.text = $"HP: {playerStats.CurrentHP:F0} / {playerStats.MaxHP.Value:F0}";
        mpText.text = $"MP: {playerStats.CurrentMP:F0} / {playerStats.MaxMP.Value:F0}";
        
        attackText.text = $"공격력: {playerStats.Attack.Value:F1}";
        magicText.text = $"마력: {playerStats.Magic.Value:F1}";
        defenseText.text = $"방어력: {playerStats.Defense.Value:F1}";
        
        moveSpeedText.text = $"이동속도: {playerStats.MoveSpeed.Value:F1}";
        attackSpeedText.text = $"공격속도: {playerStats.AttackSpeed.Value:F1}";
        // [수정] 내부 값이 0~1 범위(예: 0.1)라면 100을 곱해서 퍼센트(10%)로 표기해야 함
        cooldownText.text = $"재사용 대기시간: {playerStats.CooldownReduction.Value * 100f:F1}%";
        
        HPregenText.text = $"체력재생: {playerStats.HPRegen.Value:F1}";
        MPregenText.text = $"마나재생: {playerStats.MPRegen.Value:F1}";

        // 2. 무기 정보 갱신
        if (weaponManager != null)
        {
            var currentWeapon = weaponManager.currentWeapon;
            if (currentWeapon != null)
            {
                weaponIcon.sprite = currentWeapon.icon;
                weaponIcon.color = Color.white;
                // [수정] WeaponData(ItemData)의 설명 필드는 'description'임 (TooltipUI.cs 참조)
                weaponDesc.text = $"<b>{currentWeapon.itemName}</b>\n{currentWeapon.description}";
            }
            else
            {
                weaponIcon.sprite = null;
                weaponIcon.color = new Color(1, 1, 1, 0);
                weaponDesc.text = "무기 장착 해제됨";
            }
        }

        // 3. 원소 정보 갱신
        if (playerElement != null)
        {
            elementDesc.text = $"현재 원소: <color=yellow>{playerElement.CurrentElement}</color>";
            // elementIcon.sprite = ... 원소 아이콘 로직 필요 시 추가
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponentInChildren<PlayerStats>();
            // [수정] 컴포넌트가 자식 오브젝트에 있을 가능성을 대비해 GetComponentInChildren 사용
            playerElement = player.GetComponentInChildren<PlayerElement>();
            weaponManager = player.GetComponentInChildren<WeaponManager>();
        }
    }
}