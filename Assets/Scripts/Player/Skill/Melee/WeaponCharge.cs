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

    [Header("Visuals (이펙트)")]
    public float effectScale = 1.0f;
    public GameObject effect0;
    public GameObject effect50;
    public GameObject effect100;

    [Header("0% 전용 스프라이트")]
    public Sprite chargeSprite0;
    public Sprite attackSprite0;

    [Header("조정")]
    public float rotationOffset = 0f;

    [Header("위치 설정")]
    [Tooltip("플레이어의 오른손 오브젝트를 여기에 드래그하세요")]
    public Transform handTransform;
    [Tooltip("손 오브젝트가 없다면 사용할 수동 오프셋")]
    public Vector3 handOffset = new Vector3(0.5f, 0f, 0f);

    private float lastUsedTime = -999f;
    private PlayerStats playerStats;
    private Vector2 direction;
    private Camera mainCam;

    public float Cooldown => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + cooldown) - Time.time);

    private void Awake() { mainCam = Camera.main; }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || Time.time < lastUsedTime + cooldown) return false;
        playerStats = owner.GetComponentInChildren<PlayerStats>();
        if (playerStats == null || !playerStats.SpendMP(skillManaCost)) return false;

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
        while (elapsed < maxChargeDuration && (keyToHold == KeyCode.None || Input.GetKey(keyToHold)))
        {
            elapsed += Time.deltaTime;

            // 마우스 방향 계산의 중심점 결정
            Vector3 targetPivotPos = GetHandPosition(owner);
            UpdateDirection(targetPivotPos);

            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }

        float finalRatio = Mathf.Clamp01(elapsed / maxChargeDuration);
        GameObject targetEffect = (finalRatio >= 1.0f) ? effect100 : (finalRatio >= 0.5f ? effect50 : effect0);

        if (targetEffect != null)
        {
            targetEffect.SetActive(true);
            Vector3 spawnPos = GetHandPosition(owner);
            float angle = Mathf.Atan2(direction.y, direction.x) * RadToDeg(); // LaTeX: $RadToDeg$

            SyncPivotTransform(targetEffect, spawnPos, 1.0f, angle);
            ExecuteCircleAttack(targetEffect, owner);
            yield return StartCoroutine(PlayAnimationFull(targetEffect));
            targetEffect.SetActive(false);
        }

        activeEnhancer?.OnEnd(owner);
    }

    private float RadToDeg() => Mathf.Rad2Deg;

    private Vector3 GetHandPosition(GameObject owner)
    {
        if (handTransform != null) return handTransform.position;

        float lookDir = owner.transform.localScale.x > 0 ? 1 : -1;
        Vector3 actualOffset = new Vector3(handOffset.x * lookDir, handOffset.y, handOffset.z);
        return owner.transform.position + actualOffset;
    }

    private IEnumerator PlayAnimationFull(GameObject target)
    {
        Animator anim = target.GetComponentInChildren<Animator>();
        if (anim == null)
        {
            var sr = target.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && attackSprite0 != null) sr.sprite = attackSprite0;
            yield return new WaitForSeconds(0.3f);
            yield break;
        }

        anim.SetTrigger("OnAttack");
        yield return new WaitForEndOfFrame();

        var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
    }

    // [에러 해결됨] 매개변수 이름을 pivotPos로 일치시켰습니다.
    private void SyncPivotTransform(GameObject pivotObj, Vector3 pivotPos, float mult, float angle)
    {
        pivotObj.transform.position = pivotPos;
        pivotObj.transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        pivotObj.transform.localScale = Vector3.one * (effectScale * mult);
    }

    private void ExecuteCircleAttack(GameObject effectObj, GameObject owner)
    {
        CircleCollider2D circleCol = effectObj.GetComponentInChildren<CircleCollider2D>();
        if (circleCol == null) return;

        float actualRadius = circleCol.radius * effectObj.transform.lossyScale.x;
        Vector2 attackCenter = circleCol.bounds.center;

        int finalDamage = Mathf.RoundToInt(playerStats.Attack.Value * 1.0f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, actualRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null) health.TakeDamage(finalDamage, (enemy.transform.position - owner.transform.position).normalized);
        }
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