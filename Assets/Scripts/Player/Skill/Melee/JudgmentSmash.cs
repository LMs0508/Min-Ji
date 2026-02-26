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
    [Header("UI & Cost")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;
    public float skillManaCost = 20f;

    [Header("스킬 셋팅")]
    public float maxJumpDistance = 5f;
    public float cooldown = 10f;
    private float lastUsedTime = -999f;

    [Header("조준점 설정")]
    public GameObject landingIndicatorPrefab;
    private GameObject spawnedIndicator;

    [Header("VFX Objects (자식들)")]
    public GameObject chargeVFX;
    public GameObject riseVFX;
    public GameObject airVFX;
    public GameObject fallVFX;

    [Header("Movement Settings")]
    public float jumpHeight = 5f;
    public float riseDuration = 0.15f;
    public float pauseTime = 0.5f;
    public float fallDuration = 0.1f;

    [Header("Combat Settings")]
    public float explosionRadius = 3f;
    public float knockbackForce = 15f;

    // 클래스 멤버 변수 (런타임에 자동 할당됨)
    private SpriteRenderer playerRenderer;
    private Animator parentAnim;
    private bool isExecuting = false;

    public float Cooldown => cooldown;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + cooldown) - Time.time);

    void Start()
    {
        DisableAllVFX();
    }

    void Update()
    {
        // 렌더러가 있을 때만 레이어 정렬 업데이트
        if (playerRenderer != null) UpdateSortingOrder();
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || isExecuting) return false;
        if (Time.time < lastUsedTime + cooldown) return false;

        // [수정] owner로부터 필요한 컴포넌트들을 실시간으로 찾습니다
        var stats = owner.GetComponentInChildren<PlayerStats>();
        var runner = owner.GetComponent<CoroutineRunner>();

        // 필수 컴포넌트 체크 (하나라도 없으면 실행 불가)
        if (stats == null || runner == null) return false;

        // 마나 체크 및 소모
        if (!stats.SpendMP(skillManaCost))
        {
            Debug.Log("마나 부족: 심판의 일격 불가");
            return false;
        }

        // 성공 시 데이터 기록 및 코루틴 시작
        lastUsedTime = Time.time;
        runner.StartCoroutine(ExecuteJudgmentSmash(owner));
        return true;
    }

    private IEnumerator ExecuteJudgmentSmash(GameObject owner)
    {
        isExecuting = true;

        transform.position = owner.transform.position + new Vector3(0, 0.25f, 0);
        foreach (Transform child in transform) child.localPosition = Vector3.zero;

        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;
        var activeEnhancer = GetComponents<ISkillElementEnhancer>().FirstOrDefault(e => e.TargetElement == currentElement);

        activeEnhancer?.OnStart(owner);

        var controller = owner.GetComponentInChildren<TopDownCharacterController>();
        var rb = owner.GetComponentInChildren<Rigidbody2D>();
        parentAnim = owner.GetComponentInChildren<Animator>();
        playerRenderer = owner.GetComponent<SpriteRenderer>() ?? owner.GetComponentInChildren<SpriteRenderer>();

        if (controller != null) controller.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 1. 기 모으기
        SetVFX(chargeVFX);
        if (parentAnim != null) parentAnim.SetTrigger("OnJudgment");
        yield return new WaitForSeconds(0.2f);

        // 2. 수직 상승
        SetVFX(riseVFX);
        Vector3 startPos = owner.transform.position;
        Vector3 peakPos = startPos + Vector3.up * jumpHeight;
        yield return StartCoroutine(MoveLinear(owner.transform, startPos, peakPos, riseDuration));

        // 3. 공중 정지 및 실시간 조준
        SetVFX(airVFX);
        if (landingIndicatorPrefab != null)
        {
            spawnedIndicator = Instantiate(landingIndicatorPrefab);
            SpriteRenderer indicatorSR = spawnedIndicator.GetComponent<SpriteRenderer>();
            if (indicatorSR != null && playerRenderer != null)
            {
                indicatorSR.sortingLayerID = playerRenderer.sortingLayerID;
                indicatorSR.sortingOrder = playerRenderer.sortingOrder - 1;
            }
        }

        float elapsed = 0;
        Vector3 currentTargetPos = startPos;
        while (elapsed < pauseTime)
        {
            currentTargetPos = GetAdjustedTargetPositionByTag(startPos);
            if (spawnedIndicator != null) spawnedIndicator.transform.position = currentTargetPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. 낙하 (이 부분이 핵심 수정 사항입니다)
        if (spawnedIndicator != null) Destroy(spawnedIndicator); // 낙하 시작 시 조준점 제거
        SetVFX(fallVFX);
        if (parentAnim != null) parentAnim.SetTrigger("OnFall");

        float fallElapsed = 0;
        Vector3 fallStartPos = owner.transform.position;

        while (fallElapsed < fallDuration)
        {
            fallElapsed += Time.deltaTime;
            owner.transform.position = Vector3.Lerp(fallStartPos, currentTargetPos, fallElapsed / fallDuration);

            activeEnhancer?.OnUpdate(owner);
            yield return null;
        }
        owner.transform.position = currentTargetPos; // 착지 지점 고정

        // 5. 폭발 및 후처리 (루프 밖으로 뺐습니다)
        Explode(owner, currentTargetPos);

        yield return new WaitForSeconds(0.2f);
        DisableAllVFX();

        activeEnhancer?.OnEnd(owner);

        if (controller != null) controller.enabled = true;
        isExecuting = false;
    }

    private Vector3 GetAdjustedTargetPositionByTag(Vector3 origin)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);
        targetPos.z = 0;

        Vector2 direction = (Vector2)targetPos - (Vector2)origin;
        if (direction.magnitude > maxJumpDistance)
        {
            targetPos = origin + (Vector3)(direction.normalized * maxJumpDistance);
        }

        // 벽 레이어 체크 (Wall 레이어 마스크 사용)
        RaycastHit2D hit = Physics2D.Raycast(origin, ((Vector2)targetPos - (Vector2)origin).normalized,
      Vector2.Distance(origin, targetPos), LayerMask.GetMask("Wall"));

        if (hit.collider != null)
        {
            return (Vector3)hit.point - (Vector3)(((Vector2)targetPos - (Vector2)origin).normalized * 0.5f);
        }
        return targetPos;
    }

    private void Explode(GameObject owner, Vector3 position)
    {
        var stats = owner.GetComponentInChildren<PlayerStats>();
        float playerAttack = (stats != null) ? stats.Attack.Value : 20f;
        int finalDamage = Mathf.RoundToInt(playerAttack * 2f);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth healthScript = hit.GetComponent<EnemyHealth>();
                if (healthScript != null)
                {
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)position).normalized;
                    healthScript.TakeDamage(finalDamage, knockbackDir);

                    if (!healthScript.IsDead)
                    {
                        Animator enemyAnim = hit.GetComponentInChildren<Animator>();
                        if (enemyAnim != null) enemyAnim.SetTrigger("Hit");

                        Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector2.zero;
                            Vector2 rawDir = (Vector2)hit.transform.position - (Vector2)position;
                            Vector2 finalKnockbackDir = rawDir.magnitude < 0.1f ? Vector2.up : rawDir.normalized;
                            if (finalKnockbackDir.y > 0) finalKnockbackDir.y += 0.2f;
                            finalKnockbackDir = finalKnockbackDir.normalized;
                            rb.AddForce(finalKnockbackDir * knockbackForce, ForceMode2D.Impulse);
                        }

                        EnemyMover mover = hit.GetComponent<EnemyMover>();
                        if (mover != null) mover.ApplyStun(1.0f);
                    }
                }
            }
        }
    }

    private void UpdateSortingOrder()
    {
        SortingGroup sg = GetComponent<SortingGroup>();
        if (sg != null && playerRenderer != null)
        {
            sg.sortingLayerID = playerRenderer.sortingLayerID;
            sg.sortingOrder = playerRenderer.sortingOrder + 1;
        }
    }

    private void SetVFX(GameObject target)
    {
        DisableAllVFX();
        if (target != null) target.SetActive(true);
    }

    private void DisableAllVFX()
    {
        if (chargeVFX) chargeVFX.SetActive(false);
        if (riseVFX) riseVFX.SetActive(false);
        if (airVFX) airVFX.SetActive(false);
        if (fallVFX) fallVFX.SetActive(false);
    }

    private IEnumerator MoveLinear(Transform target, Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            target.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.position = end;
    }
}