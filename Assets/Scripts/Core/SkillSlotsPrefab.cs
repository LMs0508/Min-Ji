using UnityEngine;
using System;

public class SkillSlotsPrefab : MonoBehaviour
{
    public GameObject[] equippedObj = new GameObject[4];
    public ISkill[] equippedSkill = new ISkill[4];
    // 여기에 바닥에 떨어질 아이템 프리팹들이 저장됩니다.
    public GameObject[] equippedPickupPrefab = new GameObject[4];

    public Transform skillHolder;
    private SpriteRenderer playerSr;

    public event Action<int, GameObject> OnEquipped;
    public event Action<int, float> OnCooldownChanged;

    private void Awake()
    {
        playerSr = GetComponentInParent<SpriteRenderer>();
        if (skillHolder == null)
        {
            GameObject go = new GameObject("SkillHolder");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            skillHolder = go.transform;
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if(sr != null && playerSr != null)
        {
            sr.sortingLayerID = playerSr.sortingLayerID;
            sr.sortingOrder = playerSr.sortingOrder + 5;
        }
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // 핵심: 스킬 장착 및 교체 로직
    public void Equip(GameObject skillPrefab, GameObject pickupPrefab, int slot)
    {
        if (slot < 0 || slot >= 4) return;

        // 기존 스킬 드롭 로직
        if (equippedObj[slot] != null)
        {
            // Missing 방지 체크
            if (equippedPickupPrefab[slot] != null)
            {
                Vector3 dropPos = transform.position + new Vector3(1f, 0, 0);
                Instantiate(equippedPickupPrefab[slot], dropPos, Quaternion.identity);
                Debug.Log($"<color=yellow>{equippedPickupPrefab[slot].name} 드롭 성공</color>");
            }
            else
            {
                Debug.LogWarning("드롭할 프리팹 정보가 유실되었습니다(Missing).");
            }
            Destroy(equippedObj[slot]);
        }

        // 새 스킬 세팅
        GameObject inst = Instantiate(skillPrefab, skillHolder);
        inst.SetActive(true);

        if (playerSr != null)
        {
            SpriteRenderer[] skillSrs = inst.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var s in skillSrs)
            {
                s.sortingOrder = playerSr.sortingOrder + 10;
            }
        }
        equippedObj[slot] = inst;
        equippedSkill[slot] = inst.GetComponent<ISkill>();

        // 이 시점에 pickupPrefab이 null인지 확인
        if (pickupPrefab == null) Debug.LogError("전달받은 pickupPrefab이 이미 NULL입니다!");

        equippedPickupPrefab[slot] = pickupPrefab;

        OnEquipped?.Invoke(slot, skillPrefab);
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (equippedObj[i] != null && equippedObj[i].layer != gameObject.layer)
            {
                SetLayerRecursively(equippedObj[i], gameObject.layer);
            }
        }

        // 키 입력 처리 (Q, W, E, R)
        if (Input.GetKeyDown(KeyCode.Q)) Use(0);
        if (Input.GetKeyDown(KeyCode.W)) Use(1);
        if (Input.GetKeyDown(KeyCode.E)) Use(2);
        if (Input.GetKeyDown(KeyCode.R)) Use(3);
        HandleCooldownEvents();
    }
    private void HandleCooldownEvents()
    {
        for (int i = 0; i < 4; i++)
        {
            var s = equippedSkill[i];
            if (s == null || s.Cooldown <= 0f)
            {
                OnCooldownChanged?.Invoke(i, 0f);
                continue;
            }
            float fill = s.CooldownRemaining / s.Cooldown;
            OnCooldownChanged?.Invoke(i, fill);
        }
    }

    public void Use(int slot)
    {
        if (slot >= 0 && slot < 4 && equippedSkill[slot] != null)
        {
            equippedSkill[slot].TryUse(gameObject);
        }
    }
}