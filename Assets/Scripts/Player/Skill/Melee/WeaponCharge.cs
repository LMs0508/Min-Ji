using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Player;
using Game.Core;
using Cainos.PixelArtTopDown_Basic;

public class WeaponCharge : MonoBehaviour, ISkill
{
    [Header("공격 모션 설정")]
    public GameObject skillPlayerVisual;
    public Vector3 skillVisualOffset;

    [Header("UI 설정")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("스킬 능력치")]
    public float maxChargeDuration = 2.0f;
    public float cooldown = 5.0f;
    public float skillManaCost = 15.0f;
    public LayerMask enemyLayer;

    [Header("이펙트 설정 (각각 WeaponChargeDamage 포함)")]
    public float effectScale = 1.0f;
    public GameObject effect0, effect50, effect100;

    [Header("공격 위치 설정")]
    public string handObjectName = "RightHand";
    public Vector3 handOffset = new Vector3(0.5f, 0f, 0f);

    [Header("물리 효과")]
    public float knockbackForce = 5.0f;
    public float stunDuration = 0.5f;

    private PlayerVisualHandler visualHandler;
    private TopDownCharacterController controller;
    private float lastUsedTime = -999f;
    private PlayerStats playerStats;
    private Vector2 lookDirection = Vector2.right;
    private Camera mainCam;
    private Transform cachedHand;

    public float Cooldown => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + cooldown) - Time.time);

    private void Awake() { mainCam = Camera.main; }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || Time.time < lastUsedTime + cooldown) return false;

        playerStats = owner.GetComponentInChildren<PlayerStats>();
        if (playerStats == null || !playerStats.SpendMP(skillManaCost)) return false;

        visualHandler = owner.GetComponent<PlayerVisualHandler>();
        controller = owner.GetComponent<TopDownCharacterController>();
        cachedHand = FindChildRecursive(owner.transform, handObjectName);

        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null) return false;

        lastUsedTime = Time.time;
        runner.StartCoroutine(ChargeSequence(owner, GetCurrentPressedKey()));
        return true;
    }

    private IEnumerator ChargeSequence(GameObject owner, KeyCode keyToHold)
    {
        if (visualHandler) visualHandler.TriggerCombatMode();
        ToggleAllEffects(false, false, false);

        float elapsed = 0f;
        while (elapsed < maxChargeDuration && (keyToHold == KeyCode.None || Input.GetKey(keyToHold)))
        {
            elapsed += Time.deltaTime;
            UpdateLookDirection(owner);
            yield return null;
        }

        float finalRatio = Mathf.Clamp01(elapsed / maxChargeDuration);
        GameObject targetEffect = (finalRatio >= 1.0f) ? effect100 : (finalRatio >= 0.5f ? effect50 : effect0);
        float damageMult = (finalRatio >= 1.0f) ? 2.5f : (finalRatio >= 0.5f ? 1.5f : 1.0f);

        if (targetEffect)
        {
            if (finalRatio >= 0.5f) yield return new WaitForSeconds(0.3f);

            // [최적화] 분리된 데미지 스크립트 가져오기 및 셋업
            WeaponChargeDamage dmgScript = targetEffect.GetComponentInChildren<WeaponChargeDamage>();
            if (dmgScript) dmgScript.Setup(owner, playerStats, damageMult, knockbackForce, enemyLayer);

            if (visualHandler) visualHandler.enabled = false;
            SetNormalVisualsVisible(owner, false);

            if (skillPlayerVisual)
            {
                skillPlayerVisual.SetActive(true);
                UpdateAttackVisualTransform(owner);
                Animator vfxPlayerAnim = skillPlayerVisual.GetComponent<Animator>();
                if (vfxPlayerAnim)
                {
                    vfxPlayerAnim.speed = (targetEffect == effect0) ? 2.0f : 1.0f;
                    vfxPlayerAnim.SetTrigger("OnWeaponCharge");
                }
            }

            targetEffect.SetActive(true);
            SyncEffectTransform(targetEffect, owner);

            // [전달] 데미지 스크립트를 넘겨 매 프레임 판정 수행
            yield return StartCoroutine(PlayAttackAnimation(targetEffect, dmgScript, owner, skillPlayerVisual));

            targetEffect.SetActive(false);
            if (skillPlayerVisual) skillPlayerVisual.SetActive(false);
            if (visualHandler) visualHandler.enabled = true;
            SetNormalVisualsVisible(owner, true);
        }
    }

    private IEnumerator PlayAttackAnimation(GameObject target, WeaponChargeDamage dmgScript, GameObject owner, GameObject visual)
    {
        Animator vfxAnim = target.GetComponentInChildren<Animator>();
        float timer = 0f;

        if (vfxAnim) vfxAnim.SetTrigger("OnAttack");
        yield return new WaitForEndOfFrame();

        float duration = vfxAnim ? vfxAnim.GetCurrentAnimatorStateInfo(0).length / vfxAnim.speed : 0.5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (owner)
            {
                target.transform.position = GetHandPosition(owner);
                UpdateAttackVisualTransform(owner);
                // [호출] 분리된 스크립트의 히트 체크 실행
                if (dmgScript) dmgScript.CheckHit();
            }
            yield return null;
        }
    }

    private void UpdateLookDirection(GameObject owner)
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        float diffX = mousePos.x - owner.transform.position.x;
        lookDirection = new Vector2(diffX >= 0 ? 1f : -1f, 0);
    }

    private void UpdateAttackVisualTransform(GameObject owner)
    {
        if (skillPlayerVisual == null) return;
        Vector3 currentOffset = skillVisualOffset;
        currentOffset.x *= lookDirection.x;
        skillPlayerVisual.transform.position = owner.transform.position + currentOffset;

        Vector3 vScale = Vector3.one * effectScale;
        vScale.x *= lookDirection.x;
        skillPlayerVisual.transform.localScale = vScale;
    }

    private void SetNormalVisualsVisible(GameObject owner, bool isVisible)
    {
        foreach (Transform child in owner.transform)
        {
            if (skillPlayerVisual != null && child.gameObject == skillPlayerVisual) continue;
            if (child.name.Contains("Player") || child.name.Contains("Weapon") || child.name.Contains("Dash") || child.name.Contains("Walk"))
                child.gameObject.SetActive(isVisible);
        }
        var mainSR = owner.GetComponent<SpriteRenderer>();
        if (mainSR) mainSR.enabled = isVisible;
    }

    private void SyncEffectTransform(GameObject effectObj, GameObject owner)
    {
        effectObj.transform.position = GetHandPosition(owner);
        Vector3 effectScaleVec = Vector3.one * effectScale;
        effectScaleVec.x = Mathf.Abs(effectScaleVec.x) * lookDirection.x;
        effectObj.transform.localScale = effectScaleVec;
        effectObj.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
    }

    private Vector3 GetHandPosition(GameObject owner)
    {
        if (cachedHand != null) return cachedHand.position;
        return owner.transform.position + new Vector3(handOffset.x * lookDirection.x, handOffset.y, handOffset.z);
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

    private void ToggleAllEffects(bool s0, bool s50, bool s100)
    {
        if (effect0) effect0.SetActive(s0);
        if (effect50) effect50.SetActive(s50);
        if (effect100) effect100.SetActive(s100);
    }

    private KeyCode GetCurrentPressedKey()
    {
        if (Input.GetKey(KeyCode.Q)) return KeyCode.Q;
        if (Input.GetKey(KeyCode.W)) return KeyCode.W;
        if (Input.GetKey(KeyCode.E)) return KeyCode.E;
        if (Input.GetKey(KeyCode.R)) return KeyCode.R;
        return KeyCode.None;
    }

    private void DrawGizmoFor(GameObject effect, Color color)
    {
        if (effect == null) return;
        CircleCollider2D col = effect.GetComponentInChildren<CircleCollider2D>();
        if (col)
        {
            Gizmos.color = color;
            Vector3 center = GetHandPosition(gameObject) + (Vector3)col.offset;
            Gizmos.DrawWireSphere(center, col.radius * effectScale);
        }
    }
}