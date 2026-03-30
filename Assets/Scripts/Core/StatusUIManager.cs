using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.Player;
using Game.Core;

public class StatusUIManager : MonoBehaviour
{
    [Header("Player Reference")]
    private PlayerStats playerStats;
    private PlayerElement playerElement;
    private WeaponManager weaponManager;
    private SkillSlotsPrefab playerSkills;

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

    [Header("Skill Slot UI")]
    public Image[] skillSlotImages; // Q, W, E, R 순서대로 할당할 이미지 슬롯 (4개)
    public Sprite emptySlotSprite; // 스킬이 없을 때 표시할 기본 이미지 (선택 사항)

    [Header("Element Icons Setup")]
    public Sprite fireIcon;
    public Sprite waterIcon;
    public Sprite windIcon;
    public Sprite earthIcon;
    public Sprite noneIcon;

    [Header("UI Buttons")]
    public Button closeButton;

    void Awake()
    {
        // 닫기 버튼이 연결되어 있다면 클릭 시 창을 닫도록 설정
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void Start()
    {
        // 스킬 슬롯 UI에 대한 초기 설정을 한 번만 수행합니다.
        SetupSkillSlots();
    }

    void OnEnable()
    {
        RefreshUI();
        // 창이 활성화될 때 플레이어 스킬 변경 이벤트에 구독합니다.
        if (playerSkills != null)
        {
            playerSkills.OnEquipped += HandleSkillEquipped;
        }
    }

    void OnDisable()
    {
        // 창이 비활성화될 때 이벤트 구독을 해제하여 성능 저하를 방지합니다.
        if (playerSkills != null)
        {
            playerSkills.OnEquipped -= HandleSkillEquipped;
        }
    }

    // [수정] 창이 열려있는 동안 실시간으로 스탯 변화(체력 재생 등)를 반영하기 위해 Update 추가
    void Update()
    {
        RefreshUI();
    }

    // 스킬 슬롯에 필요한 컴포넌트의 인덱스를 자동으로 설정합니다.
    private void SetupSkillSlots()
    {
        if (skillSlotImages == null) return;
        if (playerSkills == null) FindPlayer();

        for (int i = 0; i < skillSlotImages.Length; i++)
        {
            if (skillSlotImages[i] != null)
            {
                UIItemSlot itemSlot = skillSlotImages[i].GetComponent<UIItemSlot>();
                if (itemSlot != null)
                {
                    itemSlot.slotType = UIItemSlot.SlotType.Skill;
                    itemSlot.skillIndex = i;
                }

                SkillTooltip tooltip = skillSlotImages[i].GetComponent<SkillTooltip>();
                if (tooltip != null)
                {
                    tooltip.slotIndex = i;
                    if (playerSkills != null) tooltip.Bind(playerSkills, playerElement);
                }
            }
        }
    }

    // 스킬 장착/해제 시 아이콘을 바로 갱신하기 위한 이벤트 핸들러
    private void HandleSkillEquipped(int slotIndex, GameObject skillPrefab)
    {
        UpdateSkillIcons();
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
            string colorName = playerElement.CurrentElement switch
            {
                ElementType.Fire => "red",
                ElementType.Water => "blue",
                ElementType.Wind => "green",
                ElementType.Earth => "#863d02d2",
                _ => "black"
            };

            elementDesc.text = $"현재 원소: <color={colorName}>{playerElement.CurrentElement}</color>";
            
            // [수정] PlayerElement에는 아이콘 반환 함수가 없으므로, UI에서 직접 처리
            elementIcon.sprite = playerElement.CurrentElement switch
            {
                ElementType.Fire => fireIcon,
                ElementType.Water => waterIcon,
                ElementType.Wind => windIcon,
                ElementType.Earth => earthIcon,
                _ => noneIcon
            };
        }

        // 4. 스킬 아이콘 갱신
        UpdateSkillIcons();
        if (playerSkills == null) playerSkills = GameObject.FindGameObjectWithTag("Player").GetComponent<SkillSlotsPrefab>();

    for (int i = 0; i < skillSlotImages.Length; i++)
    {
        if (i >= 4 || i >= playerSkills.equippedSkill.Length) break; // 변수명 s 제거: equippedSkill

        ISkill skill = playerSkills.equippedSkill[i]; // s 제거

        // 툴팁 컴포넌트에 실시간으로 참조를 갱신
        SkillTooltip tooltip = skillSlotImages[i].GetComponent<SkillTooltip>();
        if (tooltip != null)
        {
            tooltip.Bind(playerSkills, playerElement);
        }

        if (skill != null && skill.Icon != null)
        {
            skillSlotImages[i].sprite = skill.Icon;
            skillSlotImages[i].color = Color.white;
            skillSlotImages[i].raycastTarget = true;
        }
        else
        {
            skillSlotImages[i].sprite = emptySlotSprite;
            skillSlotImages[i].color = (emptySlotSprite == null) ? new Color(1, 1, 1, 0) : Color.white;
            skillSlotImages[i].raycastTarget = false; 
        }
    }
    }

    // 스킬 아이콘 UI를 현재 장착된 스킬에 맞게 업데이트
    private void UpdateSkillIcons()
    {
        if (playerSkills == null || skillSlotImages == null) return;

        for (int i = 0; i < skillSlotImages.Length; i++)
        {
            if (i >= playerSkills.equippedSkill.Length || skillSlotImages[i] == null) continue;

            ISkill skill = playerSkills.equippedSkill[i];

            // 툴팁 컴포넌트에 실시간으로 참조를 갱신합니다.
            SkillTooltip tooltip = skillSlotImages[i].GetComponent<SkillTooltip>();
            if (tooltip != null)
            {
                tooltip.Bind(playerSkills, playerElement);
            }

            if (skill != null && skill.Icon != null)
            {
                skillSlotImages[i].sprite = skill.Icon;
                skillSlotImages[i].color = Color.white;
                skillSlotImages[i].raycastTarget = true; // 드래그 및 툴팁을 위해 레이캐스트 활성화
            }
            else
            {
                skillSlotImages[i].sprite = emptySlotSprite;
                skillSlotImages[i].color = (emptySlotSprite == null) ? new Color(1, 1, 1, 0) : Color.white;
                skillSlotImages[i].raycastTarget = false; // 빈 슬롯은 상호작용 불가
            }
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
            playerSkills = player.GetComponent<SkillSlotsPrefab>();
        }
    }
}