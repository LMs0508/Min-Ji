using UnityEngine;
using System;

public class SkillSlotsPrefab : MonoBehaviour
{
    private GameObject[] equippedObj = new GameObject[4];
    private ISkill[] equippedSkill = new ISkill[4];

    public Transform skillHolder;

    //  UI 업데이트용 이벤트
    public event Action<int, GameObject> OnEquipped;
    public event System.Action<int, float> OnCooldownChanged; // slot, fill(0~1)

    private void Awake()
    {
        if (skillHolder == null)
        {
            var go = new GameObject("SkillHolder");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            skillHolder = go.transform;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) Use(0);
        if (Input.GetKeyDown(KeyCode.W)) Use(1);
        if (Input.GetKeyDown(KeyCode.E)) Use(2);
        if (Input.GetKeyDown(KeyCode.R)) Use(3);

        for (int i = 0; i < 4; i++)
        {
            var s = equippedSkill[i];
            if (s == null || s.Cooldown <= 0f)
            {
                OnCooldownChanged?.Invoke(i, 0f);
                continue;
            }

            float fill = s.CooldownRemaining / s.Cooldown; // 1 -> 0
            OnCooldownChanged?.Invoke(i, fill);
        }
    }

    public void Equip(GameObject skillPrefab, int slot)
    {
        if (skillPrefab == null) return;
        if (slot < 0 || slot >= 4) return;

        // 기존 장착 스킬 제거(교체)
        if (equippedObj[slot] != null)
            Destroy(equippedObj[slot]);

        var inst = Instantiate(skillPrefab, skillHolder);
        inst.SetActive(true); //  코루틴/Use 정상 위해 켜두는 게 안전

        var skill = inst.GetComponent<ISkill>();
        if (skill == null)
        {
            Debug.LogWarning("이 프리팹엔 ISkill이 없어!");
            Destroy(inst);
            return;
        }

        equippedObj[slot] = inst;
        equippedSkill[slot] = skill;

        //  UI 알림
        OnEquipped?.Invoke(slot, skillPrefab);
    }

    public void Use(int slot)
    {
        var skill = equippedSkill[slot];
        if (skill == null) return;
        skill.TryUse(gameObject);
    }
}