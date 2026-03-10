using UnityEngine;
using System;

public class SkillSlotsPrefab : MonoBehaviour
{
    public GameObject[] equippedObj = new GameObject[4];
    public ISkill[] equippedSkill = new ISkill[4];
    public GameObject[] equippedPickupPrefab = new GameObject[4];

    [Header("스킬 게이지 UI (Q, W, E, R 순서대로 4개)")]
    public SkillGaugeUI[] slotGauges = new SkillGaugeUI[4];

    public Transform skillHolder;
    public event Action<int, GameObject> OnEquipped;
    public event Action<int, float> OnCooldownChanged;

    private void Awake()
    {
        if (skillHolder == null)
        {
            GameObject go = new GameObject("SkillHolder");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            skillHolder = go.transform;
        }
    }

    public void Equip(GameObject skillPrefab, GameObject pickupPrefab, int slot)
    {
        if (slot < 0 || slot >= 4) return;

        if (equippedObj[slot] != null)
        {
            if (equippedPickupPrefab[slot] != null)
            {
                Vector3 dropPos = transform.position + new Vector3(1f, 0, 0);
                Instantiate(equippedPickupPrefab[slot], dropPos, Quaternion.identity);
            }
            Destroy(equippedObj[slot]);
        }

        GameObject inst = Instantiate(skillPrefab, skillHolder);
        inst.SetActive(true);

        equippedObj[slot] = inst;
        equippedSkill[slot] = inst.GetComponent<ISkill>();
        equippedPickupPrefab[slot] = pickupPrefab;

        // [게이지 연결] 스킬 장착 시점에 자동으로 슬롯에 맞는 UI 연결
        ConnectGaugeToSkill(inst, slot);

        OnEquipped?.Invoke(slot, skillPrefab);
    }

    private void ConnectGaugeToSkill(GameObject inst, int slot)
    {
        if (slotGauges.Length <= slot || slotGauges[slot] == null) return;

        // WeaponCharge 체크
        //var wc = inst.GetComponent<WeaponCharge>();
        //if (wc != null) wc.chargeGaugeUI = slotGauges[slot];

        // DashEarthEnhancer 체크 (인핸서가 여러개일 수 있으므로 GetComponent 확인)
        var earthDash = inst.GetComponent<DashEarthEnhancer>();
        if (earthDash != null) earthDash.dashGaugeUI = slotGauges[slot];
    }

    private void Update()
    {
        bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        // Ctrl이 눌려있지 않을 때만 Q, W, E, R 입력으로 스킬 사용
        if (!isCtrlPressed)
        {
            if (Input.GetKeyDown(KeyCode.Q)) Use(0);
            if (Input.GetKeyDown(KeyCode.W)) Use(1);
            if (Input.GetKeyDown(KeyCode.E)) Use(2);
            if (Input.GetKeyDown(KeyCode.R)) Use(3);
        }

        // 쿨다운 업데이트 (기존의 비율 계산 방식 유지)
        for (int i = 0; i < 4; i++)
        {
            var s = equippedSkill[i];
            if (s == null || s.Cooldown <= 0f)
            {
                OnCooldownChanged?.Invoke(i, 0f);
                continue;
            }

            // [중요] 남은 시간 / 전체 시간 비율로 계산하여 UI에 전달
            float fill = s.CooldownRemaining / s.Cooldown;
            OnCooldownChanged?.Invoke(i, fill);
        }
    }

    public void Use(int slot)
    {
        if (slot >= 0 && slot < 4 && equippedSkill[slot] != null)
        {
            // 스킬 사용 시도
            bool success = equippedSkill[slot].TryUse(gameObject);

            // 성공했다면 즉시 쿨타임 UI가 반응하도록 이벤트 발송
            if (success)
            {
                float fill = equippedSkill[slot].CooldownRemaining / equippedSkill[slot].Cooldown;
                OnCooldownChanged?.Invoke(slot, fill);
            }
        }
    }
}