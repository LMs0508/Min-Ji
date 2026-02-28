using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Player;
using Game.Core;

public class WeaponCharge : MonoBehaviour, ISkill
{
    [Header("UI")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("스킬 설정")]
    public float maxChargeDuration = 2.0f;
    public float cooldown = 5f;
    public float skillManaCost = 15f;
    public LayerMask enemyLayer;

    [Header("Visuals (공격 범위 설정)")]
    public GameObject rangeIndicator;
    public Vector2 baseAttackSize = new Vector2(3f, 1.5f);

    private float lastUsedTime = -999f;
    private PlayerStats playerStats;
    private Vector2 direction;

    public float Cooldown => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + cooldown) - Time.time);

    public bool TryUse(GameObject owner)
    {
        if (owner == null) return false;
        if (Time.time < lastUsedTime + cooldown) return false;

        playerStats = owner.GetComponentInChildren<PlayerStats>();
        if (playerStats == null || !playerStats.SpendMP(skillManaCost)) return false;

        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null) return false;

        lastUsedTime = Time.time;
        runner.StartCoroutine(ChargeSequence(owner));
        return true;
    }

    private IEnumerator ChargeSequence(GameObject owner)
    {
        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;

        var enhancers = GetComponents<ISkillElementEnhancer>();
        ISkillElementEnhancer activeEnhancer = enhancers.FirstOrDefault(e => e.TargetElement == currentElement);

        activeEnhancer?.OnStart(owner);

        if (rangeIndicator != null) rangeIndicator.SetActive(true);

        float elapsed = 0f;
        while (elapsed < maxChargeDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / maxChargeDuration);

            UpdateDirection(owner.transform.position);
            UpdateRangeVisual(owner.transform.position, ratio);

            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }

        FireSkill(owner);

        if (rangeIndicator != null) rangeIndicator.SetActive(false);
        activeEnhancer?.OnEnd(owner);
    }

    private void UpdateDirection(Vector3 playerPos)
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z));
        direction = ((Vector2)worldMousePos - (Vector2)playerPos).normalized;
        if (direction == Vector2.zero) direction = Vector2.down;
    }

    private void UpdateRangeVisual(Vector3 playerPos, float ratio)
    {
        float visualMult = (ratio >= 1.0f) ? 1.5f : (ratio >= 0.75f ? 1.3f : 1.0f);

        if (rangeIndicator != null)
        {
            rangeIndicator.transform.localScale = new Vector3(baseAttackSize.x * visualMult, baseAttackSize.y * visualMult, 1f);

            float currentLength = baseAttackSize.x * visualMult;
            rangeIndicator.transform.position = playerPos + (Vector3)direction * (currentLength / 2f);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rangeIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void FireSkill(GameObject owner)
    {
        ExecuteAttack(0.8f, 1.5f, owner);
    }

    private void ExecuteAttack(float damageMult, float rangeMult, GameObject owner)
    {
        int finalDamage = Mathf.RoundToInt(playerStats.Attack.Value * damageMult);

        Vector2 attackSize = new Vector2(baseAttackSize.x * rangeMult, baseAttackSize.y * rangeMult);

        Vector2 attackPos = (Vector2)owner.transform.position + (direction * (attackSize.x / 2f));

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPos, attackSize, angle, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                Vector2 knockback = (enemy.transform.position - owner.transform.position).normalized;
                health.TakeDamage(finalDamage, knockback);
            }
        }
    }
}