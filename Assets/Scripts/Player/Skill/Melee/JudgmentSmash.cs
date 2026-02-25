using System.Collections;
using UnityEngine;
using UnityEngine.Rendering; // Sorting Group 사용 시 필요

public class JudgmentSmash : MonoBehaviour
{
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
    public int damage = 50;
    public float knockbackForce = 15f;

    private bool isCooldown = false;

    void Start()
    {
        DisableAllVFX();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isCooldown)
        {
            StartCoroutine(ExecuteJudgmentSmash());
        }

        // 실시간 레이어 순서 동기화
        UpdateSortingOrder();
    }

    IEnumerator ExecuteJudgmentSmash()
    {
        isCooldown = true;

        // 1. 기 모으기
        SetVFX(chargeVFX);
        if (parentAnim != null) parentAnim.SetTrigger("OnJudgment");
        yield return new WaitForSeconds(0.2f);

        // 2. 수직 상승
        SetVFX(riseVFX);
        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + Vector3.up * jumpHeight;
        yield return StartCoroutine(MoveLinear(startPos, peakPos, riseDuration));

        // 3. 공중 정지 및 조준
        SetVFX(airVFX);
        yield return new WaitForSeconds(pauseTime);

        // 4. 낙하지점 계산 (벽 태그 체크)
        Vector3 targetLandPos = GetAdjustedTargetPositionByTag();
        SetVFX(fallVFX);
        if (parentAnim != null) parentAnim.SetTrigger("OnFall");

        yield return StartCoroutine(MoveLinear(transform.position, targetLandPos, fallDuration));

        // 5. 폭발(데미지 판정)
        Explode(targetLandPos);

        yield return new WaitForSeconds(0.2f);
        DisableAllVFX();

        isCooldown = false;
    }

    private Vector3 GetAdjustedTargetPositionByTag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(mousePos);
        targetPos.z = 0;

        Vector2 startPos = transform.position;
        Vector2 direction = ((Vector2)targetPos - startPos);
        float distance = direction.magnitude;

        // 모든 레이어를 대상으로 레이를 쏩니다.
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction.normalized, distance);

        foreach (var hit in hits)
        {
            // 부딪힌 물체의 태그가 "Wall"인 경우
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                // 벽 발견! 벽에서 약간 떨어진 지점을 반환합니다.
                return (Vector3)hit.point - (Vector3)(direction.normalized * 0.5f);
            }
        }

        return targetPos;
    }

    void Explode(Vector3 position)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, explosionRadius);

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth healthScript = hit.GetComponent<EnemyHealth>();
                if (healthScript != null)
                {
                    Vector2 knockbackDir = (hit.transform.position - position).normalized;
                    healthScript.TakeDamage(damage, knockbackDir);
                }

                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 forceDir = (hit.transform.position - position).normalized;
                    rb.AddForce(forceDir * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    // --- 유틸리티 및 레이어 설정 ---

    void UpdateSortingOrder()
    {
        if (playerRenderer == null) return;

        // 부모 오브젝트에 Sorting Group이 있다면 그것을 조절하고,
        // 없다면 각 VFX의 SpriteRenderer를 조절합니다.
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