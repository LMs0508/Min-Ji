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
        SetPlayerCoreVisual(owner, false);

        // [핵심 변경] 애니메이션과 VFX를 켤 때 항상 플레이어 위치(owner.transform)를 기준으로 켭니다.
        SetVFX(chargeVFX, owner.transform);
        SetSkillSprite(spriteA, owner.transform);
        yield return new WaitForSeconds(0.2f);

        SetVFX(riseVFX, owner.transform);
        SetSkillSprite(spriteB, owner.transform);
        Vector3 startPos = owner.transform.position;
        Vector3 peakPos = startPos + Vector3.up * jumpHeight;
        yield return StartCoroutine(MoveLinear(owner.transform, startPos, peakPos, riseDuration));

        SetVFX(airVFX, owner.transform);
        float elapsed = 0;
        Vector3 currentTargetPos = startPos;
        while (elapsed < pauseTime)
        {
            currentTargetPos = GetAdjustedTargetPositionByTag(startPos);
            if (spawnedIndicator != null) spawnedIndicator.transform.position = currentTargetPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (spawnedIndicator != null) Destroy(spawnedIndicator);

        SetSkillSprite(null, owner.transform);
        SetVFX(fallVFX, owner.transform);

        float actualAnimDuration = 0.5f;
        Animator fallAnim = fallVFX.GetComponent<Animator>();
        if (fallAnim != null)
        {
            // [복구 완료] 불 속성 오류를 일으켰던 애니메이션 이름 강제 지정(Play) 코드를 제거했습니다.
            yield return null;
            actualAnimDuration = fallAnim.GetCurrentAnimatorStateInfo(0).length;
        }

        if (parentAnim != null) parentAnim.SetTrigger("OnFall");

        Vector3 fallStartPos = owner.transform.position;
        float distance = Vector3.Distance(fallStartPos, currentTargetPos);
        float physicalMoveDuration = distance / fallSpeed;

        float totalWaitTime = Mathf.Max(physicalMoveDuration, actualAnimDuration);
        float fallTimeCounter = 0f;
        bool hasHitGround = false;

        while (fallTimeCounter < totalWaitTime)
        {
            fallTimeCounter += Time.deltaTime;

            if (!hasHitGround)
            {
                float moveT = physicalMoveDuration > 0 ? fallTimeCounter / physicalMoveDuration : 1f;
                owner.transform.position = Vector3.Lerp(fallStartPos, currentTargetPos, Mathf.Min(moveT, 1f));

                if (moveT >= 1f)
                {
                    owner.transform.position = currentTargetPos;
                    hasHitGround = true;
                }
            }
            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }

        Explode(owner, currentTargetPos);
        DisableAllVFX();

        SetSkillSprite(spriteC, owner.transform);
        yield return new WaitForSeconds(0.5f);

        SetSkillSprite(null, owner.transform);
        RestorePlayerVisual(owner, controller);

        activeEnhancer?.OnEnd(owner);
        if (enhancerInst != null) Destroy(enhancerInst);

        isExecuting = false;
    }

    private void RestorePlayerVisual(GameObject owner, TopDownCharacterController controller)
    {
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

    // [핵심 변경] 스킬 스프라이트를 켤 때 플레이어의 좌표로 강제 정렬합니다.
    private void SetSkillSprite(GameObject target, Transform referenceTransform)
    {
        if (spriteA) spriteA.SetActive(false);
        if (spriteB) spriteB.SetActive(false);
        if (spriteC) spriteC.SetActive(false);

        if (target != null)
        {
            target.transform.position = referenceTransform.position;
            target.SetActive(true);
        }
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
                        if (enemyAnim != null) enemyAnim.SetTrigger("Hit");

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

    // [핵심 변경] VFX를 켤 때 플레이어의 좌표로 강제 정렬합니다.
    private void SetVFX(GameObject target, Transform referenceTransform)
    {
        DisableAllVFX();
        if (target != null)
        {
            target.transform.position = referenceTransform.position;
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

    private IEnumerator MoveLinear(Transform target, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration) { target.position = Vector3.Lerp(start, end, elapsed / duration); elapsed += Time.deltaTime; yield return null; }
        target.position = end;
    }
}