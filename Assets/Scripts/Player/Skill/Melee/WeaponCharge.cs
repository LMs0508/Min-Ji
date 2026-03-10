using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Player;
using Game.Core;

public class WeaponCharge : MonoBehaviour, ISkill
{
    [Header("UI")]
    public SkillGaugeUI chargeGaugeUI; // 여기에 게이지 프리팹/오브젝트 연결

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("스킬 설정")]
    public float maxChargeDuration = 2.0f;
    public float cooldown = 5f;
    public float skillManaCost = 15f;
    public LayerMask enemyLayer;

    [Header("Visuals (이펙트)")]
    public float effectScale = 1.0f;
    public GameObject effect0, effect50, effect100;

    [Header("조정")]
    public float rotationOffset = 0f;

    [Header("위치 설정 (자동 검색)")]
    public string handObjectName = "RightHand";
    public Vector3 handOffset = new Vector3(0.5f, 0f, 0f);

    [Header("물리 설정 (속성 강화용)")]
    public float knockbackForce = 0f;
    public float stunDuration = 0f;

    private float lastUsedTime = -999f;
    private PlayerStats playerStats;
    private Vector2 direction;
    private Camera mainCam;
    private Transform cachedHand;
    private HashSet<GameObject> hitEnemiesHistory = new HashSet<GameObject>();

    public float Cooldown => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + cooldown) - Time.time);

    private void Awake() { mainCam = Camera.main; }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || Time.time < lastUsedTime + cooldown) return false;
        playerStats = owner.GetComponentInChildren<PlayerStats>();
        if (playerStats == null || !playerStats.SpendMP(skillManaCost)) return false;

        cachedHand = FindChildRecursive(owner.transform, handObjectName);
        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null) return false;

        KeyCode pressedKey = GetCurrentPressedKey();
        lastUsedTime = Time.time;
        runner.StartCoroutine(ChargeSequence(owner, pressedKey));
        return true;
    }

    private IEnumerator ChargeSequence(GameObject owner, KeyCode keyToHold)
    {
        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;
        var activeEnhancer = GetComponents<ISkillElementEnhancer>().FirstOrDefault(e => e.TargetElement == currentElement);

        activeEnhancer?.OnStart(owner);
        ToggleAllEffects(false, false, false);

        float elapsed = 0f;
        // [수정] 차징 시작 시 게이지 초기화
        if (chargeGaugeUI != null) chargeGaugeUI.SetGauge(0, maxChargeDuration);

        while (elapsed < maxChargeDuration && (keyToHold == KeyCode.None || Input.GetKey(keyToHold)))
        {
            elapsed += Time.deltaTime;

            // [수정] 매 프레임 게이지 UI 업데이트 (0 -> 2초로 차오름)
            if (chargeGaugeUI != null) chargeGaugeUI.SetGauge(elapsed, maxChargeDuration);

            UpdateDirection(GetHandPosition(owner));
            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }

        // 차징 종료 후 게이지 숨기기
        if (chargeGaugeUI != null) chargeGaugeUI.Hide();

        float finalRatio = Mathf.Clamp01(elapsed / maxChargeDuration);
        GameObject targetEffect = (finalRatio >= 1.0f) ? effect100 : (finalRatio >= 0.5f ? effect50 : effect0);
        float damageMult = (finalRatio >= 1.0f) ? 2.5f : (finalRatio >= 0.5f ? 1.5f : 1.0f);

        if (targetEffect != null)
        {
            targetEffect.SetActive(true);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            SyncPivotTransform(targetEffect, GetHandPosition(owner), angle);

            yield return StartCoroutine(PlayAnimationWithFollow(targetEffect, owner, damageMult));

            targetEffect.SetActive(false);
            knockbackForce = 0f;
            stunDuration = 0f;
        }
        activeEnhancer?.OnEnd(owner);
    }

    private IEnumerator PlayAnimationWithFollow(GameObject target, GameObject owner, float damageMult)
    {
        Animator anim = target.GetComponentInChildren<Animator>();
        float timer = 0f, duration = 0.5f;
        hitEnemiesHistory.Clear();

        if (anim != null)
        {
            anim.SetTrigger("OnAttack");
            yield return new WaitForEndOfFrame();
            duration = anim.GetCurrentAnimatorStateInfo(0).length;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (owner != null)
            {
                target.transform.position = GetHandPosition(owner);
                ExecuteContinuousAttack(target, owner, damageMult);
            }
            yield return null;
        }
    }

    private void ExecuteContinuousAttack(GameObject effectObj, GameObject owner, float damageMult)
    {
        Physics2D.SyncTransforms();
        CircleCollider2D circleCol = effectObj.GetComponentInChildren<CircleCollider2D>();
        if (circleCol == null) return;

        Vector2 attackCenter = effectObj.transform.TransformPoint(circleCol.offset);
        float actualRadius = circleCol.radius * effectObj.transform.lossyScale.x;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, actualRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (hitEnemiesHistory.Contains(enemy.gameObject)) continue;

            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                int finalDamage = Mathf.RoundToInt(playerStats.Attack.Value * damageMult);
                Vector2 rawDir = (enemy.transform.position - owner.transform.position);
                Vector2 knockbackDir = rawDir.magnitude < 0.1f ? Vector2.up : rawDir.normalized;

                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                if (rb != null && knockbackForce > 0)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }

                enemyHealth.TakeDamage(finalDamage); // 수정된 EnemyHealth에 맞춰 호출
                hitEnemiesHistory.Add(enemy.gameObject);
            }
        }
    }

    private Vector3 GetHandPosition(GameObject owner)
    {
        if (cachedHand != null) return cachedHand.position;
        float lookDir = owner.transform.localScale.x > 0 ? 1 : -1;
        return owner.transform.position + new Vector3(handOffset.x * lookDir, handOffset.y, handOffset.z);
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void SyncPivotTransform(GameObject pivotObj, Vector3 pivotPos, float angle)
    {
        pivotObj.transform.position = pivotPos;
        pivotObj.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        pivotObj.transform.localScale = Vector3.one * effectScale;
    }

    private void UpdateDirection(Vector3 pivotPos)
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 worldMousePos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCam.transform.position.z));
        direction = ((Vector2)worldMousePos - (Vector2)pivotPos).normalized;
    }

    private void ToggleAllEffects(bool s0, bool s50, bool s100)
    {
        if (effect0 != null) effect0.SetActive(s0);
        if (effect50 != null) effect50.SetActive(s50);
        if (effect100 != null) effect100.SetActive(s100);
    }

    private KeyCode GetCurrentPressedKey()
    {
        if (Input.GetKey(KeyCode.Q)) return KeyCode.Q;
        if (Input.GetKey(KeyCode.W)) return KeyCode.W;
        if (Input.GetKey(KeyCode.E)) return KeyCode.E;
        if (Input.GetKey(KeyCode.R)) return KeyCode.R;
        return KeyCode.None;
    }
}