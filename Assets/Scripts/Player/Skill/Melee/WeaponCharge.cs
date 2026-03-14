using System.Collections;
using System.Linq;
using UnityEngine;
using Game.Player;
using Game.Core;

public class WeaponCharge : MonoBehaviour, ISkill
{
    [Header("Skill Data (스크립터블 오브젝트 할당)")]
    public SkillData skillData;

    [Header("UI & Cost")]
    public float skillManaCost = 15f;
    [HideInInspector] public SkillGaugeUI chargeGaugeUI;

    public Sprite Icon => skillData != null ? skillData.icon : null;
    public float Cooldown => skillData != null ? skillData.cooldown : 0f;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + Cooldown) - Time.time);

    [Header("시각 효과 (VFX)")]
    public GameObject chargingVFX;

    [Header("0% Settings (Stage 0)")]
    public GameObject obj0Percent;
    public GameObject playerAnim0Percent;
    public float duration0Percent = 0.5f;

    [Header("50% Settings (Stage 1)")]
    public GameObject obj50Percent;
    public GameObject playerAnim50Percent;
    public float duration50Percent = 1.5f;

    [Header("100% Settings (Stage 2)")]
    public GameObject obj100Percent;
    public GameObject playerAnim100Percent;
    public float duration100Percent = 2.0f;

    [Header("Combat Options")]
    public float knockbackForce = 10f;
    public float stunDuration = 0.5f;

    private float lastUsedTime = -999f;
    private bool isCharging = false;
    private bool isExecuting = false;

    private SpriteRenderer playerRenderer;
    private int currentCalculatedDamage;
    private Vector2 currentAttackDirection;

    private void Awake()
    {
        DisableAllVisuals();
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || isCharging || isExecuting || skillData == null) return false;
        if (Time.time < lastUsedTime + Cooldown) return false;

        var stats = owner.GetComponentInChildren<PlayerStats>();
        var runner = owner.GetComponent<CoroutineRunner>();

        if (stats == null || runner == null) return false;
        if (!stats.SpendMP(skillManaCost)) return false;

        playerRenderer = owner.GetComponent<SpriteRenderer>() ?? owner.GetComponentInChildren<SpriteRenderer>();

        runner.StartCoroutine(ChargeRoutine(owner, stats));
        return true;
    }

    private IEnumerator ChargeRoutine(GameObject owner, PlayerStats stats)
    {
        isCharging = true;
        float chargeTimer = 0f;

        float maxChargeTime = 2f;
        if (skillData.stageTimeThresholds != null && skillData.stageTimeThresholds.Length > 0)
        {
            maxChargeTime = skillData.stageTimeThresholds[skillData.stageTimeThresholds.Length - 1];
        }

        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;
        var activeEnhancer = GetComponents<ISkillElementEnhancer>().FirstOrDefault(e => e.TargetElement == currentElement);

        activeEnhancer?.OnStart(owner);

        KeyCode activeKey = KeyCode.None;
        if (Input.GetKey(KeyCode.Q)) activeKey = KeyCode.Q;
        else if (Input.GetKey(KeyCode.W)) activeKey = KeyCode.W;
        else if (Input.GetKey(KeyCode.E)) activeKey = KeyCode.E;
        else if (Input.GetKey(KeyCode.R)) activeKey = KeyCode.R;

        if (chargeGaugeUI != null)
        {
            chargeGaugeUI.Show();
            chargeGaugeUI.SetGauge(0, maxChargeTime);
        }

        // [수정된 부분] 켜지기 1프레임 전부터 플레이어 위치로 완벽하게 고정시킴
        if (chargingVFX != null)
        {
            chargingVFX.transform.position = owner.transform.position;
            chargingVFX.SetActive(true);
        }

        while (activeKey != KeyCode.None && Input.GetKey(activeKey))
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0f, maxChargeTime);

            if (chargeGaugeUI != null) chargeGaugeUI.SetGauge(chargeTimer, maxChargeTime);
            activeEnhancer?.OnUpdate(owner);

            // [수정된 부분] 매 프레임 플레이어 위치를 쫓아가며, 마우스 방향에 맞춰 차징 이펙트도 좌우 반전
            if (chargingVFX != null)
            {
                chargingVFX.transform.position = owner.transform.position;

                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                bool isFacingRight = mousePos.x >= owner.transform.position.x;
                Vector3 vfxScale = chargingVFX.transform.localScale;
                vfxScale.x = Mathf.Abs(vfxScale.x) * (isFacingRight ? 1f : -1f);
                chargingVFX.transform.localScale = vfxScale;
            }

            yield return null;
        }

        if (chargingVFX != null)
        {
            chargingVFX.SetActive(false);
        }

        float chargePercent = chargeTimer / maxChargeTime;

        if (activeEnhancer is WeaponChargeWindEnhancer windEnhancer)
        {
            windEnhancer.ApplyWindChargeEffect(chargePercent);
        }

        if (chargeGaugeUI != null) chargeGaugeUI.Hide();
        isCharging = false;

        yield return StartCoroutine(ExecuteAttackRoutine(owner, stats, chargeTimer, activeEnhancer));
    }

    private IEnumerator ExecuteAttackRoutine(GameObject owner, PlayerStats stats, float chargeTimer, ISkillElementEnhancer activeEnhancer)
    {
        isExecuting = true;

        SetPlayerCoreVisual(owner, false);

        int stageIndex = 0;
        GameObject activeParentObj = null;
        GameObject activePlayerAnim = null;
        float currentAnimDuration = 0f;

        if (skillData.stageTimeThresholds.Length >= 3)
        {
            if (chargeTimer >= skillData.stageTimeThresholds[2])
            {
                stageIndex = 2;
                activeParentObj = obj100Percent;
                activePlayerAnim = playerAnim100Percent;
                currentAnimDuration = duration100Percent;
            }
            else if (chargeTimer >= skillData.stageTimeThresholds[1])
            {
                stageIndex = 1;
                activeParentObj = obj50Percent;
                activePlayerAnim = playerAnim50Percent;
                currentAnimDuration = duration50Percent;
            }
            else
            {
                stageIndex = 0;
                activeParentObj = obj0Percent;
                activePlayerAnim = playerAnim0Percent;
                currentAnimDuration = duration0Percent;
            }
        }

        float baseDamage = stats.Attack.Value * skillData.damageRatio;
        float currentMultiplier = skillData.stageMultipliers.Length > stageIndex ? skillData.stageMultipliers[stageIndex] : 1f;
        currentCalculatedDamage = Mathf.RoundToInt(baseDamage * currentMultiplier);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 attackOrigin = owner.transform.position;

        bool isFacingRight = mousePos.x >= attackOrigin.x;
        currentAttackDirection = isFacingRight ? Vector2.right : Vector2.left;

        if (activeParentObj != null)
        {
            activeParentObj.transform.position = attackOrigin;

            Vector3 effectScale = activeParentObj.transform.localScale;
            effectScale.x = Mathf.Abs(effectScale.x) * (isFacingRight ? 1f : -1f);
            activeParentObj.transform.localScale = effectScale;
            activeParentObj.transform.rotation = Quaternion.identity;

            activeParentObj.SetActive(true);

            Animator skillAnim = activeParentObj.GetComponent<Animator>();
            if (skillAnim == null) skillAnim = activeParentObj.GetComponentInChildren<Animator>();

            SafeSetTrigger(skillAnim, "OnAttack");
        }

        if (activePlayerAnim != null)
        {
            activePlayerAnim.transform.position = attackOrigin;

            Vector3 playerAnimScale = activePlayerAnim.transform.localScale;
            playerAnimScale.x = Mathf.Abs(playerAnimScale.x) * (isFacingRight ? 1f : -1f);
            activePlayerAnim.transform.localScale = playerAnimScale;

            activePlayerAnim.SetActive(true);

            Animator playerAnim = activePlayerAnim.GetComponent<Animator>();
            if (playerAnim == null) playerAnim = activePlayerAnim.GetComponentInChildren<Animator>();

            SafeSetTrigger(playerAnim, "OnAttack");
        }

        yield return new WaitForSeconds(currentAnimDuration);

        DisableAllVisuals();
        SetPlayerCoreVisual(owner, true);

        var visualHandler = owner.GetComponent<PlayerVisualHandler>();
        if (visualHandler != null) visualHandler.TriggerCombatMode();

        activeEnhancer?.OnEnd(owner);

        lastUsedTime = Time.time;
        isExecuting = false;
    }

    public void PerformHitboxDamage(Collider2D enemyCollider)
    {
        EnemyHealth healthScript = enemyCollider.GetComponent<EnemyHealth>();
        if (healthScript != null)
        {
            healthScript.TakeDamage(currentCalculatedDamage, currentAttackDirection);

            Rigidbody2D rb = enemyCollider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(currentAttackDirection * knockbackForce, ForceMode2D.Impulse);
            }

            if (!healthScript.IsDead)
            {
                Animator enemyAnim = enemyCollider.GetComponentInChildren<Animator>();
                SafeSetTrigger(enemyAnim, "Hit");

                EnemyMover mover = enemyCollider.GetComponent<EnemyMover>();
                if (mover != null) mover.ApplyStun(stunDuration);
            }
        }
    }

    private void DisableAllVisuals()
    {
        if (chargingVFX != null) chargingVFX.SetActive(false);
        if (obj0Percent) obj0Percent.SetActive(false);
        if (obj50Percent) obj50Percent.SetActive(false);
        if (obj100Percent) obj100Percent.SetActive(false);

        if (playerAnim0Percent) playerAnim0Percent.SetActive(false);
        if (playerAnim50Percent) playerAnim50Percent.SetActive(false);
        if (playerAnim100Percent) playerAnim100Percent.SetActive(false);
    }

    private void SetPlayerCoreVisual(GameObject owner, bool show)
    {
        if (playerRenderer != null) playerRenderer.enabled = show;

        foreach (Transform child in owner.transform)
        {
            if (child.name == "SkillHolder" || child.name == "Shadow" || child.gameObject == gameObject) continue;
            child.gameObject.SetActive(show);
        }
    }

    private void SafeSetTrigger(Animator anim, string triggerName)
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == triggerName)
            {
                anim.SetTrigger(triggerName);
                break;
            }
        }
    }
}