using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChargedAttackSkill : MonoBehaviour, ISkill
{
    public SkillData skillData;
    public LayerMask enemyLayer;
    public GameObject attackAreaObject;
    public Collider2D areaCollider;

    private float currentCooldown = 0f;
    private bool isCharging = false;
    private int assignedSlot = -1; // ���� �� �� �������� ����

    // ISkill �������̽� ����
    public Sprite Icon => skillData.icon;
    public float Cooldown => skillData.cooldown;
    public float CooldownRemaining => currentCooldown;

    private void Update()
    {
        if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

        // ��¡ ���� ���� Ű �Է��� �ǽð����� üũ
        if (isCharging && assignedSlot != -1)
        {
            CheckKeyRelease();
        }
    }

    public bool TryUse(GameObject owner)
    {
        if (currentCooldown > 0 || isCharging) return false;

        // 1. ���� �� �� ���Կ� �ִ��� ã�� (SkillSlotsPrefab�� �迭���� ���� ã��)
        SkillSlotsPrefab slots = owner.GetComponent<SkillSlotsPrefab>();
        for (int i = 0; i < 4; i++)
        {
            if (slots.equippedObj[i] == gameObject)
            {
                assignedSlot = i;
                break;
            }
        }

        // 2. ��¡ ����
        StartCoroutine(ChargeRoutine(owner));
        return true;
    }

    private KeyCode GetSlotKey(int slot)
    {
        switch (slot)
        {
            case 0: return KeyCode.Q;
            case 1: return KeyCode.W;
            case 2: return KeyCode.E;
            case 3: return KeyCode.R;
            default: return KeyCode.None;
        }
    }

    private float chargeStartTime;
    private bool keyReleased = false;

    private IEnumerator ChargeRoutine(GameObject owner)
    {
        isCharging = true;
        keyReleased = false;
        chargeStartTime = Time.time;

        // Ű�� �� ������ ��� (Update���� keyReleased�� �ٲ��� ����)
        yield return new WaitUntil(() => keyReleased);

        float chargeTime = Time.time - chargeStartTime;
        ExecuteAttack(owner, chargeTime);

        currentCooldown = skillData.cooldown;
        isCharging = false;
    }

    private void CheckKeyRelease()
    {
        KeyCode myKey = GetSlotKey(assignedSlot);
        // �ش� ������ Ű�� ����
        if (Input.GetKeyUp(myKey))
        {
            keyReleased = true;
        }
    }

    private void ExecuteAttack(GameObject owner, float chargeTime)
    {
        // ���콺 �������� ȸ��
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 dir = (mousePos - owner.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float damage = 1f;
        float rangeScale = 1f;
        int level = 1;

        if (chargeTime < 0.5f) { level = 1; damage = 1f; rangeScale = 1f; }
        else if (chargeTime < 1.5f) { level = 2; damage = 2f; rangeScale = 2f; }
        else { level = 3; damage = 2f; rangeScale = 3f; }

        attackAreaObject.transform.localScale = new Vector3(rangeScale, rangeScale, 1);

        if (level == 3) StartCoroutine(DotAttackRoutine(1f, 0.2f));
        else StartCoroutine(InstantAttackRoutine(damage));
    }

    // ... (���� InstantAttackRoutine, DotAttackRoutine, ApplyDamage�� ������ ����)
    private IEnumerator InstantAttackRoutine(float damage)
    {
        attackAreaObject.SetActive(true);
        ApplyDamage(damage);
        yield return new WaitForSeconds(0.1f);
        attackAreaObject.SetActive(false);
    }

    private IEnumerator DotAttackRoutine(float duration, float interval)
    {
        attackAreaObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            ApplyDamage(0.5f);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        attackAreaObject.SetActive(false);
    }

    private void ApplyDamage(float damage)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;
        List<Collider2D> results = new List<Collider2D>();
        areaCollider.Overlap(filter, results);
        foreach (var enemy in results)
        {
            if (enemy.TryGetComponent(out EnemyHealth health)) health.TakeDamage(damage);
        }
    }
}