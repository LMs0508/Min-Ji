using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Cainos.PixelArtTopDown_Basic;
using Game.Player;
using Game.Core;

public class JudgmentSmash : MonoBehaviour, ISkill
{
    [Header("Skill Data (스크립터블 오브젝트 할당)")]
    public SkillData skillData;

    [Header("UI & Cost")]
    public float skillManaCost = 20f;
    public Sprite Icon => skillData != null ? skillData.icon : null;

    [Header("스킬 전용 플레이어 외형")]
    public GameObject spriteA;
    public GameObject spriteB;
    public GameObject spriteC;

    [Header("스킬 셋팅")]
    public float maxJumpDistance = 5f;
    private float lastUsedTime = -999f;

    [Header("조준점 설정")]
    public GameObject landingIndicatorPrefab;
    private GameObject spawnedIndicator;

    [Header("VFX Objects (여기에 Normal 이펙트를 넣으세요)")]
    public GameObject chargeVFX;
    public GameObject riseVFX;
    public GameObject airVFX;
    public GameObject fallVFX;

    [HideInInspector] public GameObject defaultCharge, defaultRise, defaultAir, defaultFall;

    [Header("Movement Settings")]
    public float jumpHeight = 5f;
    public float riseDuration = 0.15f;
    public float pauseTime = 0.5f;
    public float fallSpeed = 30f;

    [Header("Combat Settings")]
    public float explosionRadius = 3f;
    public float knockbackForce = 15f;

    private SpriteRenderer playerRenderer;
    private Animator parentAnim;
    private bool isExecuting = false;

    public float Cooldown => skillData != null ? skillData.cooldown : 0f;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + Cooldown) - Time.time);

    void Awake()
    {
        defaultCharge = chargeVFX; defaultRise = riseVFX;
        defaultAir = airVFX; defaultFall = fallVFX;
    }

    void Start()
    {
        DisableAllVFX();
    }

    void Update()
    {
        if (playerRenderer != null) UpdateSortingOrder();
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || isExecuting || skillData == null) return false;
        if (Time.time < lastUsedTime + Cooldown) return false;

        var stats = owner.GetComponentInChildren<PlayerStats>();
        var runner = owner.GetComponent<CoroutineRunner>();

        if (stats == null || runner == null) return false;

        if (!stats.SpendMP(skillManaCost)) return false;

        lastUsedTime = Time.time;
        runner.StartCoroutine(ExecuteJudgmentSmash(owner));
        return true;
    }

    private IEnumerator ExecuteJudgmentSmash(GameObject owner)
    {
        isExecuting = true;

        var controller = owner.GetComponent<TopDownCharacterController>();
        var rb = owner.GetComponentInChildren<Rigidbody2D>();
        parentAnim = owner.GetComponentInChildren<Animator>();
        playerRenderer = owner.GetComponent<SpriteRenderer>() ?? owner.GetComponentInChildren<SpriteRenderer>();

        // [추가] 무적 상태를 위해 플레이어의 모든 콜라이더를 찾아서 끕니다.
        Collider2D[] playerColliders = owner.GetComponentsInChildren<Collider2D>();
        foreach (var col in playerColliders) col.enabled = false;

        ISkillElementEnhancer activeEnhancer = null;
        GameObject enhancerInst = null;

        if (skillData != null && skillData.isElementReactive)
        {
            var playerElement = owner.GetComponentInChildren<PlayerElement>();
            ElementType currentElement = playerElement != null && playerElement.HasElement ? playerElement.CurrentElement : skillData.defaultElement;

            GameObject targetEnhancerPrefab = null;

            switch (currentElement)
            {
                case ElementType.Fire: targetEnhancerPrefab = skillData.fireEnhancerPrefab; break;
                case ElementType.Water: targetEnhancerPrefab = skillData.waterEnhancerPrefab; break;
                case ElementType.Earth: targetEnhancerPrefab = skillData.earthEnhancerPrefab; break;
                case ElementType.Wind: targetEnhancerPrefab = skillData.windEnhancerPrefab; break;
            }

            if (targetEnhancerPrefab != null)
            {
                enhancerInst = Instantiate(targetEnhancerPrefab, transform);
                activeEnhancer = enhancerInst.GetComponent<ISkillElementEnhancer>();
            }
        }

        if (activeEnhancer == null)
        {
            var playerElement = owner.GetComponentInChildren<PlayerElement>();
            ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;
            activeEnhancer = GetComponents<ISkillElementEnhancer>().FirstOrDefault(e => e.TargetElement == currentElement);
        }

        activeEnhancer?.OnStart(owner);

        if (controller != null) controller.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        SetPlayerCoreVisual(owner, false); // 본체 투명화

        Vector3 startPos = owner.transform.position;
        Vector3 peakPos = startPos + Vector3.up * jumpHeight;
        Vector3 currentVisualPos = startPos; // 가상의 스프라이트/VFX 위치

        // [1. Charge Phase]
        SetVFX(chargeVFX, currentVisualPos);
        SetSkillSprite(spriteA, currentVisualPos);
        yield return new WaitForSeconds(0.2f);

        // [2. Rise Phase (상승)] 본체는 두고 시각 요소만 위로 올립니다.
        SetVFX(riseVFX, currentVisualPos);
        SetSkillSprite(spriteB, currentVisualPos);
        float elapsed = 0;
        while (elapsed < riseDuration)
        {
            currentVisualPos = Vector3.Lerp(startPos, peakPos, elapsed / riseDuration);
            UpdateActiveVisualsPosition(currentVisualPos);
            elapsed += Time.deltaTime;
            yield return null;
        }
        currentVisualPos = peakPos;
        UpdateActiveVisualsPosition(currentVisualPos);

        // [3. Air Phase (공중 체공)] 시각 요소는 공중에 고정.
        SetVFX(airVFX, currentVisualPos);
        elapsed = 0;
        Vector3 currentTargetPos = startPos;
        while (elapsed < pauseTime)
        {
            currentTargetPos = GetAdjustedTargetPositionByTag(startPos);
            if (spawnedIndicator != null) spawnedIndicator.transform.position = currentTargetPos;

            UpdateActiveVisualsPosition(currentVisualPos);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spawnedIndicator != null) Destroy(spawnedIndicator);

        // [4. Fall Phase (하강)]
        UpdateVisualFlip(owner.transform.position, currentTargetPos);
        SetSkillSprite(null, currentVisualPos);
        SetVFX(fallVFX, currentVisualPos);

        float actualAnimDuration = 0.5f;
        Animator fallAnim = fallVFX.GetComponent<Animator>();
        if (fallAnim != null)
        {
            yield return null;
            actualAnimDuration = fallAnim.GetCurrentAnimatorStateInfo(0).length;
        }

        SafeSetTrigger(parentAnim, "OnFall");

        Vector3 fallStartPos = peakPos; // 하강 시작 위치는 공중
        float distance = Vector3.Distance(fallStartPos, currentTargetPos);
        float physicalMoveDuration = distance / fallSpeed;

        float totalWaitTime = Mathf.Max(physicalMoveDuration, actualAnimDuration);
        float fallTimeCounter = 0f;

        while (fallTimeCounter < totalWaitTime)
        {
            fallTimeCounter += Time.deltaTime;
            float moveT = physicalMoveDuration > 0 ? fallTimeCounter / physicalMoveDuration : 1f;

            // 시각 요소만 목표 지점(currentTargetPos)으로 하강시킵니다.
            currentVisualPos = Vector3.Lerp(fallStartPos, currentTargetPos, Mathf.Min(moveT, 1f));
            UpdateActiveVisualsPosition(currentVisualPos);

            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }

        owner.transform.position = currentTargetPos;

        Explode(owner, currentTargetPos);
        DisableAllVFX();

        SetSkillSprite(spriteC, currentTargetPos);
        yield return new WaitForSeconds(0.5f);

        SetSkillSprite(null, currentTargetPos);
        RestorePlayerVisual(owner, controller);

        // [복구] 스킬이 끝났으므로 콜라이더를 켜서 다시 타격받을 수 있게 합니다.
        foreach (var col in playerColliders) col.enabled = true;

        activeEnhancer?.OnEnd(owner);
        if (enhancerInst != null) Destroy(enhancerInst);

        isExecuting = false;
    }

    private void UpdateVisualFlip(Vector3 startPos, Vector3 targetPos)
    {
        // 목표 지점이 시작 지점보다 왼쪽이면 -1 (반전), 오른쪽이면 1 (정방향)
        float flipX = (targetPos.x < startPos.x) ? -1f : 1f;

        // 모든 시각 요소 오브젝트의 Scale X값을 수정
        ApplyFlip(spriteA, flipX);
        ApplyFlip(spriteB, flipX);
        ApplyFlip(spriteC, flipX);

        ApplyFlip(chargeVFX, flipX);
        ApplyFlip(riseVFX, flipX);
        ApplyFlip(airVFX, flipX);
        ApplyFlip(fallVFX, flipX);
    }

    private void ApplyFlip(GameObject obj, float flipX)
    {
        if (obj != null)
        {
            Vector3 scale = obj.transform.localScale;
            // 절대값에 방향을 곱하여 크기는 유지하고 방향만 바꿉니다.
            scale.x = Mathf.Abs(scale.x) * flipX;
            obj.transform.localScale = scale;
        }
    }

    private void RestorePlayerVisual(GameObject owner, TopDownCharacterController controller)
    {
        UpdateVisualFlip(Vector3.zero, Vector3.right);

        if (playerRenderer != null) playerRenderer.enabled = true;
        foreach (Transform child in owner.transform)
        {
            if (child.name == "SkillHolder" || child.gameObject == gameObject) continue;

            child.gameObject.SetActive(true);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }
        if (controller != null)
        {
            controller.enabled = true;
            controller.StopMovement();
            owner.SendMessage("StopAndShowIdle", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void SetPlayerCoreVisual(GameObject owner, bool show)
    {
        if (playerRenderer != null) playerRenderer.enabled = show;

        if (!show)
        {
            foreach (Transform child in owner.transform)
            {
                if (child.name == "SkillHolder" || child.gameObject == gameObject) continue;
                child.gameObject.SetActive(false);
            }
        }
    }

    // [변경] Transform 대신 Vector3 좌표를 받아 직접 위치를 설정하도록 수정했습니다.
    private void SetSkillSprite(GameObject target, Vector3 pos)
    {
        if (spriteA) spriteA.SetActive(false);
        if (spriteB) spriteB.SetActive(false);
        if (spriteC) spriteC.SetActive(false);

        if (target != null)
        {
            target.transform.position = pos;
            target.SetActive(true);
        }
    }

    // [추가] 매 프레임마다 현재 켜져있는 이펙트와 스프라이트들의 위치를 동기화합니다.
    private void UpdateActiveVisualsPosition(Vector3 pos)
    {
        if (spriteA && spriteA.activeSelf) spriteA.transform.position = pos;
        if (spriteB && spriteB.activeSelf) spriteB.transform.position = pos;
        if (spriteC && spriteC.activeSelf) spriteC.transform.position = pos;

        if (chargeVFX && chargeVFX.activeSelf) chargeVFX.transform.position = pos;
        if (riseVFX && riseVFX.activeSelf) riseVFX.transform.position = pos;
        if (airVFX && airVFX.activeSelf) airVFX.transform.position = pos;
        if (fallVFX && fallVFX.activeSelf) fallVFX.transform.position = pos;
    }

    private void Explode(GameObject owner, Vector3 position)
    {
        var stats = owner.GetComponentInChildren<PlayerStats>();
        float playerAttack = (stats != null) ? stats.Attack.Value : 20f;

        float damageMultiplier = skillData != null ? skillData.damageRatio : 2.0f;
        int finalDamage = Mathf.RoundToInt(playerAttack * damageMultiplier);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth healthScript = hit.GetComponent<EnemyHealth>();
                if (healthScript != null)
                {
                    Vector2 rawDir = (Vector2)hit.transform.position - (Vector2)position;
                    Vector2 finalKnockbackDir = rawDir.magnitude < 0.1f ? Vector2.up : rawDir.normalized;
                    if (finalKnockbackDir.y > 0) finalKnockbackDir.y += 0.2f;

                    healthScript.TakeDamage(finalDamage, finalKnockbackDir);

                    Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.AddForce(finalKnockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
                    }

                    if (!healthScript.IsDead)
                    {
                        Animator enemyAnim = hit.GetComponentInChildren<Animator>();
                        SafeSetTrigger(enemyAnim, "Hit");
                        
                        EnemyMover mover = hit.GetComponent<EnemyMover>();
                        if (mover != null) mover.ApplyStun(1.0f);
                    }
                }
            }
        }
    }

    private Vector3 GetAdjustedTargetPositionByTag(Vector3 origin)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);
        targetPos.z = 0;
        Vector2 direction = (Vector2)targetPos - (Vector2)origin;
        if (direction.magnitude > maxJumpDistance) targetPos = origin + (Vector3)(direction.normalized * maxJumpDistance);
        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, Vector2.Distance(origin, targetPos));
        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            return (Vector3)hit.point - (Vector3)(direction.normalized * 0.5f);
        }
        return targetPos;
    }

    private void UpdateSortingOrder()
    {
        SortingGroup sg = GetComponent<SortingGroup>();
        if (sg != null && playerRenderer != null) { sg.sortingLayerID = playerRenderer.sortingLayerID; sg.sortingOrder = playerRenderer.sortingOrder + 1; }
    }

    private void SetVFX(GameObject target, Vector3 pos)
    {
        DisableAllVFX();
        if (target != null)
        {
            target.transform.position = pos;
            target.SetActive(true);
        }
    }

    private void DisableAllVFX()
    {
        if (defaultCharge) defaultCharge.SetActive(false); if (defaultRise) defaultRise.SetActive(false);
        if (defaultAir) defaultAir.SetActive(false); if (defaultFall) defaultFall.SetActive(false);
        if (chargeVFX) chargeVFX.SetActive(false); if (riseVFX) riseVFX.SetActive(false);
        if (airVFX) airVFX.SetActive(false); if (fallVFX) fallVFX.SetActive(false);
    }

    private void SafeSetTrigger(Animator anim, string triggerName)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(triggerName);
                break;
            }
        }
    }
}