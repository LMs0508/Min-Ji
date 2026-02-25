using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Cainos.PixelArtTopDown_Basic; // 이동 컨트롤러 네임스페이스
using Game.Player;                  // PlayerStats 네임스페이스

public class JudgmentSmash : MonoBehaviour
{
    [Header("스킬 셋팅")]
    public float maxJumpDistance = 5f;      // 최대 도약 거리
    public float cooldown = 10f;            // 쿨타임 10초
    private float lastUsedTime = -100f;     // 마지막 사용 시간

    [Header("조준점 설정")]
    public GameObject landingIndicatorPrefab; // 조준점 프리팹
    private GameObject spawnedIndicator;      // 생성된 조준점 인스턴스

    [Header("Animator")]
    [SerializeField] private Animator parentAnim;

    [Header("Sorting Settings")]
    [SerializeField] private SpriteRenderer playerRenderer;

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
    [SerializeField] private int baseDamage = 50; // 스텟을 못 찾을 경우를 대비한 기본 데미지

    // 외부(UI 등)에서 쿨타임 확인용
    public float CooldownRemaining => Mathf.Max(0, (lastUsedTime + cooldown) - Time.time);

    void Start()
    {
        DisableAllVFX();
    }

    void Update()
    {
        // 쿨타임 체크 후 G키 입력 시 발동
        if (Input.GetKeyDown(KeyCode.G) && Time.time >= lastUsedTime + cooldown)
        {
            StartCoroutine(ExecuteJudgmentSmash());
        }

        // 실시간 캐릭터 레이어 순서 동기화
        UpdateSortingOrder();
    }

    IEnumerator ExecuteJudgmentSmash()
    {
        lastUsedTime = Time.time; // 쿨타임 시작

        // 이동 스크립트 및 물리 일시 중지
        var controller = GetComponentInParent<TopDownCharacterController>();
        var rb = GetComponentInParent<Rigidbody2D>();
        if (controller != null) controller.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 1. 기 모으기 (Charge)
        SetVFX(chargeVFX);
        if (parentAnim != null) parentAnim.SetTrigger("OnJudgment");
        yield return new WaitForSeconds(0.2f);

        // 2. 수직 상승 (Rise)
        SetVFX(riseVFX);
        Vector3 startPos = transform.position; // 도약 시작 지점 저장
        Vector3 peakPos = startPos + Vector3.up * jumpHeight;
        yield return StartCoroutine(MoveLinear(startPos, peakPos, riseDuration));

        // 3. 공중 정지 및 실시간 조준 (Air/Targeting)
        SetVFX(airVFX);

        // 조준점 생성 및 레이어 설정
        if (landingIndicatorPrefab != null)
        {
            spawnedIndicator = Instantiate(landingIndicatorPrefab);
            SpriteRenderer indicatorSR = spawnedIndicator.GetComponent<SpriteRenderer>();
            if (indicatorSR != null && playerRenderer != null)
            {
                indicatorSR.sortingLayerID = playerRenderer.sortingLayerID;
                indicatorSR.sortingOrder = playerRenderer.sortingOrder - 1; // 발밑에 오도록
            }
        }

        float elapsed = 0;
        Vector3 currentTargetPos = startPos;

        while (elapsed < pauseTime)
        {
            currentTargetPos = GetAdjustedTargetPositionByTag(startPos);

            if (spawnedIndicator != null)
                spawnedIndicator.transform.position = currentTargetPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. 낙하 시작 (Fall)
        SetVFX(fallVFX);
        if (parentAnim != null) parentAnim.SetTrigger("OnFall");

        yield return StartCoroutine(MoveLinear(transform.position, currentTargetPos, fallDuration));

        // 5. 폭발 (Explode)
        Explode(currentTargetPos);

        if (spawnedIndicator != null) Destroy(spawnedIndicator);

        yield return new WaitForSeconds(0.2f);
        DisableAllVFX();

        // 이동 다시 허용
        if (controller != null) controller.enabled = true;
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

        Vector2 limitedDir = (Vector2)targetPos - (Vector2)origin;
        RaycastHit2D hit = Physics2D.Raycast(origin, limitedDir.normalized, limitedDir.magnitude, LayerMask.GetMask("Wall"));

        if (hit.collider != null)
        {
            return (Vector3)hit.point - (Vector3)(limitedDir.normalized * 0.5f);
        }

        return targetPos;
    }

    void Explode(Vector3 position)
    {
        // 1. 플레이어 오브젝트 및 스텟 찾기
        var playerObj = GameObject.FindWithTag("Player");
        PlayerStats stats = (playerObj != null) ? playerObj.GetComponent<PlayerStats>() : GetComponentInParent<PlayerStats>();

        // 2. 데미지 계산: 현재 공격력의 2배 (중복 선언 제거됨)
        float playerAttack = (stats != null) ? stats.Attack.Value : baseDamage;
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(playerAttack * 2f));

        Debug.Log($"[심판의 일격] 공격력: {playerAttack} -> 최종 데미지: {finalDamage} (스텟 발견: {stats != null})");

        // 3. 범위 내 적 감지
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, explosionRadius);

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                // 데미지 전달
                EnemyHealth healthScript = hit.GetComponent<EnemyHealth>();
                if (healthScript != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - position).normalized;
                    healthScript.TakeDamage(finalDamage, knockbackDir);
                }

                // 물리 넉백
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 forceDir = (hit.transform.position - position).normalized;
                    rb.AddForce(forceDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    void UpdateSortingOrder()
    {
        if (playerRenderer == null) return;
        SortingGroup sg = GetComponent<SortingGroup>();
        if (sg != null)
        {
            sg.sortingLayerID = playerRenderer.sortingLayerID;
            sg.sortingOrder = playerRenderer.sortingOrder + 1;
        }
    }

    void SetVFX(GameObject target)
    {
        DisableAllVFX();
        if (target != null) target.SetActive(true);
    }

    void DisableAllVFX()
    {
        if (chargeVFX) chargeVFX.SetActive(false);
        if (riseVFX) riseVFX.SetActive(false);
        if (airVFX) airVFX.SetActive(false);
        if (fallVFX) fallVFX.SetActive(false);
    }

    IEnumerator MoveLinear(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}