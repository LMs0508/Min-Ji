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
    public GameObject effect0, effect50, effect100;

    [Header("위치 설정 (자동 검색)")]
    public string handObjectName = "RightHand";
    public Vector3 handOffset = new Vector3(0.5f, 0f, 0f);

    [Header("물리 설정")]
    public float knockbackForce = 0f;
    public float stunDuration = 0f;

    // 내부 관리용 (런타임 자동 할당)
    private GameObject characterRight;
    private GameObject characterLeft;
    private GameObject withWeaponRoot;
    private GameObject dashRoot; // [추가] Dash 폴더 제어용

    private float lastUsedTime = -999f;
    private PlayerStats playerStats;
    private Vector2 direction = Vector2.right;
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

        // 1. 동적 오브젝트 검색 (중첩 구조 대응)
        Transform withWeaponT = FindChildRecursive(owner.transform, "WithWeapon");
        if (withWeaponT != null) withWeaponRoot = withWeaponT.gameObject;

        Transform dashT = FindChildRecursive(owner.transform, "Dash");
        if (dashT != null) dashRoot = dashT.gameObject;

        Transform rightT = FindChildRecursive(owner.transform, "Player_Dash_WithWeapon(Right)");
        Transform leftT = FindChildRecursive(owner.transform, "Player_Dash_WithWeapon(Left)");

        if (rightT != null) characterRight = rightT.gameObject;
        if (leftT != null) characterLeft = leftT.gameObject;
        cachedHand = FindChildRecursive(owner.transform, handObjectName);

        if (characterRight == null || characterLeft == null)
        {
            Debug.LogError("Dash 자식 오브젝트를 찾지 못했습니다! 이름을 확인하세요.");
            return false;
        }

        var runner = owner.GetComponent<CoroutineRunner>();
        if (runner == null) return false;

        KeyCode pressedKey = GetCurrentPressedKey();
        lastUsedTime = Time.time;
        runner.StartCoroutine(ChargeSequence(owner, pressedKey));
        return true;
    }

    private IEnumerator ChargeSequence(GameObject owner, KeyCode keyToHold)
    {
        var controller = owner.GetComponent<TopDownCharacterController>();
        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;
        var activeEnhancer = GetComponents<ISkillElementEnhancer>().FirstOrDefault(e => e.TargetElement == currentElement);

        activeEnhancer?.OnStart(owner);
        ToggleAllEffects(false, false, false);

        // --- 1. 기 모으기 ---
        float elapsed = 0f;
        while (elapsed < maxChargeDuration && (keyToHold == KeyCode.None || Input.GetKey(keyToHold)))
        {
            elapsed += Time.deltaTime;
            UpdateDirectionHorizontal(owner, false);
            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }

        // --- 2. 발사 준비 ---
        float finalRatio = Mathf.Clamp01(elapsed / maxChargeDuration);
        GameObject targetEffect = (finalRatio >= 1.0f) ? effect100 : (finalRatio >= 0.5f ? effect50 : effect0);
        float damageMult = (finalRatio >= 1.0f) ? 2.5f : (finalRatio >= 0.5f ? 1.5f : 1.0f);

        if (targetEffect != null)
        {
            UpdateDirectionHorizontal(owner, true);
            if (controller != null) controller.enabled = false;

            // [핵심 수정] 50% 이상 차지했을 때만 0.3초 대기 (선딜레이)
            if (finalRatio >= 0.5f)
            {
                yield return new WaitForSeconds(0.3f);
            }

            // 본체 숨기기 (공격 비주얼로 교체하기 위함)
            SetPlayerVisibility(false);

            // 0% 공격 시 2배속 적용
            float playbackSpeed = (targetEffect == effect0) ? 2.0f : 1.0f;

            if (skillPlayerVisual != null)
            {
                skillPlayerVisual.SetActive(true);

                Vector3 currentOffset = skillVisualOffset;
                currentOffset.x *= direction.x;
                skillPlayerVisual.transform.position = owner.transform.position + currentOffset;

                Vector3 vScale = Vector3.one * effectScale;
                vScale.x *= direction.x;
                skillPlayerVisual.transform.localScale = vScale;

                Animator vfxPlayerAnim = skillPlayerVisual.GetComponent<Animator>();
                if (vfxPlayerAnim != null)
                {
                    vfxPlayerAnim.speed = playbackSpeed;
                    vfxPlayerAnim.SetTrigger("OnWeaponCharge");
                }
            }

            targetEffect.SetActive(true);
            Animator effectAnim = targetEffect.GetComponentInChildren<Animator>();
            if (effectAnim != null) effectAnim.speed = playbackSpeed;

            // 이펙트 위치 및 회전 설정 (X축 180도 회전 적용됨)
            SyncEffectTransformHorizontal(targetEffect, owner);

            yield return StartCoroutine(PlayAnimationWithFollow(targetEffect, owner, skillPlayerVisual, damageMult));

            // --- 3. 복구 단계 ---
            if (skillPlayerVisual) skillPlayerVisual.GetComponent<Animator>().speed = 1.0f;
            if (effectAnim) effectAnim.speed = 1.0f;

            targetEffect.SetActive(false);
            if (skillPlayerVisual) skillPlayerVisual.SetActive(false);

            Rigidbody2D rb = owner.GetComponentInChildren<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            ResetPlayerAnimator(owner);
            SetPlayerVisibility(true);

            if (controller != null) controller.enabled = true;
            yield return null;
            UpdateDirectionHorizontal(owner, true);
        }
        activeEnhancer?.OnEnd(owner);
    }

    private void SetPlayerVisibility(bool isVisible)
    {
        if (withWeaponRoot == null) return;

        // WithWeapon 폴더 내 모든 컴포넌트 제어
        foreach (var anim in withWeaponRoot.GetComponentsInChildren<Animator>(true)) anim.enabled = isVisible;
        foreach (var sr in withWeaponRoot.GetComponentsInChildren<SpriteRenderer>(true)) sr.enabled = isVisible;

        withWeaponRoot.SetActive(isVisible);

        // 다시 켤 때의 초기화: 모든 '손주' 레벨까지 싹 다 꺼줍니다.
        if (isVisible)
        {
            if (dashRoot) dashRoot.SetActive(false); // Dash 폴더 일단 끄기

            // WithWeapon의 직계 자식들도 끄기 (Idle 등)
            foreach (Transform child in withWeaponRoot.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateDirectionHorizontal(GameObject owner, bool forceFlip)
    {
        if (withWeaponRoot == null) return;

        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        float diffX = mousePos.x - owner.transform.position.x;
        float lookDir = diffX >= 0 ? 1f : -1f;
        direction = new Vector2(lookDir, 0);

        // 1. 필요한 오브젝트들 참조
        GameObject idleObj = null;
        Transform idleT = FindChildRecursive(withWeaponRoot.transform, "Player_Idle_WithWeapon");
        if (idleT != null) idleObj = idleT.gameObject;

        Rigidbody2D rb = owner.GetComponentInChildren<Rigidbody2D>();
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        bool isInputting = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;
        bool isMoving = (rb != null && rb.linearVelocity.magnitude > 0.1f) || isInputting;

        if (!forceFlip && isMoving) return;

        // 2. 초기화 (모든 가능성 끄기)
        if (idleObj) idleObj.SetActive(false);
        if (dashRoot) dashRoot.SetActive(false); // Dash 폴더 끄기
        if (characterRight) characterRight.SetActive(false);
        if (characterLeft) characterLeft.SetActive(false);

        // 3. 상태에 맞춰 켜기
        if (isMoving)
        {
            if (dashRoot) dashRoot.SetActive(true); // [핵심] Dash 폴더를 먼저 켭니다.

            GameObject targetDash = (lookDir > 0) ? characterRight : characterLeft;
            if (targetDash != null)
            {
                targetDash.SetActive(true);
                Animator anim = targetDash.GetComponent<Animator>();
                if (anim != null && anim.runtimeAnimatorController != null)
                {
                    anim.SetBool("IsMoving", true);
                    anim.SetFloat("MoveX", moveX);
                    anim.SetFloat("MoveY", moveY);
                }
            }
        }
        else
        {
            if (idleObj)
            {
                idleObj.SetActive(true);
                SpriteRenderer idleSR = idleObj.GetComponent<SpriteRenderer>();
                if (idleSR) idleSR.flipX = (lookDir < 0);
            }
        }
    }

    // ... [ResetPlayerAnimator, PlayAnimationWithFollow, ExecuteContinuousAttack 등은 이전과 동일] ...
    private void ResetPlayerAnimator(GameObject owner)
    {
        Animator[] allAnims = owner.GetComponentsInChildren<Animator>(true);
        foreach (var anim in allAnims)
        {
            if (anim == null || anim.runtimeAnimatorController == null) continue;
            if (skillPlayerVisual != null && anim.gameObject == skillPlayerVisual) continue;

            if (HasParameter(anim, "IsMoving")) anim.SetBool("IsMoving", false);
            if (HasParameter(anim, "MoveX")) anim.SetFloat("MoveX", 0);
            if (HasParameter(anim, "MoveY")) anim.SetFloat("MoveY", 0);
            anim.ResetTrigger("OnWeaponCharge");
        }
    }

    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    private IEnumerator PlayAnimationWithFollow(GameObject target, GameObject owner, GameObject visual, float damageMult)
    {
        Animator vfxAnim = target.GetComponentInChildren<Animator>();
        Animator playerAnim = (visual != null) ? visual.GetComponent<Animator>() : null;
        float timer = 0f;
        hitEnemiesHistory.Clear();

        if (vfxAnim != null) vfxAnim.SetTrigger("OnAttack");
        yield return new WaitForEndOfFrame();

        float vfxLen = vfxAnim != null ? vfxAnim.GetCurrentAnimatorStateInfo(0).length / vfxAnim.speed : 0.5f;
        float playerLen = playerAnim != null ? playerAnim.GetCurrentAnimatorStateInfo(0).length / playerAnim.speed : 0.5f;
        float duration = Mathf.Max(vfxLen, playerLen);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (owner != null)
            {
                target.transform.position = GetHandPosition(owner);
                if (visual != null)
                {
                    Vector3 currentOffset = skillVisualOffset;
                    currentOffset.x *= direction.x;
                    visual.transform.position = owner.transform.position + currentOffset;
                }
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
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
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
                health.TakeDamage(finalDamage, knockbackDir);
                hitEnemiesHistory.Add(enemy.gameObject);
            }
        }
    }

    private void SyncEffectTransformHorizontal(GameObject effectObj, GameObject owner)
    {
        effectObj.transform.position = GetHandPosition(owner);
        Vector3 effectScaleVec = Vector3.one * effectScale;
        effectScaleVec.x = Mathf.Abs(effectScaleVec.x) * direction.x;
        effectObj.transform.localScale = effectScaleVec;
        effectObj.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
    }

    private Vector3 GetHandPosition(GameObject owner)
    {
        if (cachedHand != null) return cachedHand.position;
        return owner.transform.position + new Vector3(handOffset.x * direction.x, handOffset.y, handOffset.z);
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