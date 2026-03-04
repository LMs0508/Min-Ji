using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    private EnemyStats stats;
    private SpriteRenderer sr;

    [Header("UI Settings")]
    [SerializeField] private Slider hpSlider;

    public float currentHealth;
    public bool isHit = false;
    private bool isDead = false;
    public bool IsDead => isDead;

    [Header("VFX")]
    public GameObject damageTextPrefab;

    [Header("Defence Visual & Hit Stop")]
    [SerializeField] private GameObject defenceVisual;
    [SerializeField] private float hitStopDuration = 0.3f; // 히트스탑 시간
    private Animator enemyAnim;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        sr = GetComponentInChildren<SpriteRenderer>();
        enemyAnim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        currentHealth = stats.maxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = stats.maxHealth;
            hpSlider.value = currentHealth;
        }
        Canvas hpCanvas = hpSlider.GetComponentInParent<Canvas>();
        if (hpCanvas != null)
        {
            hpCanvas.worldCamera = Camera.main;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;
        HandleDamage(damage, Vector2.zero);
    }

    public void TakeDamage(float damage, Vector2 knockbackDir)
    {
        if (currentHealth <= 0) return;
        HandleDamage(damage, knockbackDir);
    }

    // [개선] 공통 데미지 처리 로직
    private void HandleDamage(float damage, Vector2 knockbackDir)
    {
        currentHealth -= damage;
        if (hpSlider != null) hpSlider.value = currentHealth;
        ShowDamageText(damage);

        // [핵심] 조우 중이 아닐 때만 피격 연출(움찔, 방어막 등)을 실행합니다.
        EnemyEncounter encounter = GetComponentInChildren<EnemyEncounter>();
        if (encounter != null && !encounter.IsEncountering)
        {
            StopCoroutine("HitFeedback");
            StartCoroutine("HitFeedback");
        }

        if (currentHealth <= 0) Die();
    }

    // [통합] 피격 피드백 코루틴 (일시정지 + 색상 + 방어막)
    // [통합] 피격 피드백 코루틴 (일시정지 + 피격 오브젝트 붉게 만들기)
    private IEnumerator HitFeedback()
    {
        isHit = true;

        // 1. Rigidbody2D 가져오기 및 물리 일시정지
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 savedVelocity = Vector2.zero;

        if (rb != null)
        {
            savedVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // 2. 애니메이션 일시정지 (현재 상태 고정)
        if (enemyAnim != null) enemyAnim.speed = 0f;

        // 3. 비주얼 교체: 본체 숨기고 피격용 오브젝트 켜기
        if (sr != null) sr.enabled = false;

        if (defenceVisual != null)
        {
            defenceVisual.SetActive(true); // 피격 스프라이트 오브젝트 활성화

            // [핵심] 피격 오브젝트 내부의 모든 스프라이트를 붉게 물들임
            SpriteRenderer[] hitSrs = defenceVisual.GetComponentsInChildren<SpriteRenderer>();
            foreach (var s in hitSrs)
            {
                s.color = Color.red;
            }
        }

        // --- 히트 스탑 대기 ---
        yield return new WaitForSeconds(hitStopDuration);

        // 4. 물리 및 애니메이션 복구
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = savedVelocity;
        }
        if (enemyAnim != null) enemyAnim.speed = 1f;

        // 5. 비주얼 원복: 본체 보이기 및 피격 오브젝트 초기화 후 끄기
        if (sr != null) sr.enabled = true;

        if (defenceVisual != null)
        {
            // 색상을 다시 하얗게 돌려놓고 비활성화 (다음에 또 써야 하니까요)
            SpriteRenderer[] hitSrs = defenceVisual.GetComponentsInChildren<SpriteRenderer>();
            foreach (var s in hitSrs)
            {
                s.color = Color.white;
            }
            defenceVisual.SetActive(false);
        }

        isHit = false;
    }

    // --- 이하 Die 및 기타 로직은 동일하지만 defenceVisual 처리 추가 ---

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. 모든 코루틴 즉시 중단 (피격/조우 연출 중지)
        StopAllCoroutines();

        // 2. 물리 및 행동 "완전 봉쇄"
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // FreezeAll은 애니메이션을 멈출 수 있으므로, FreezeRotation만 사용하거나
            // 아래 scripts 비활성화 후 simulated = false를 쓰는 것이 가장 안전합니다.
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.simulated = false; // 물리 엔진에서 완전히 제외 (공격 판정 방지)
        }

        // 3. 충돌체 즉시 차단 (더 이상 플레이어와 부딪히거나 공격받지 않음)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. 모든 스크립트(Mover, Attack 등) 비활성화
        // [핵심] 여기서 스크립트가 꺼져야 Update에서의 방향 전환과 공격 명령이 멈춥니다.
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script == this || script is Animator) continue;
            script.enabled = false;
        }

        // 5. 비주얼 복구 (투명화 방지)
        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
        if (defenceVisual != null) defenceVisual.SetActive(false);
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
        }

        // 6. [해결] 애니메이터 파라미터 강제 초기화 및 재생
        if (enemyAnim != null)
        {
            enemyAnim.enabled = true;
            enemyAnim.speed = 1f;

            // 모든 공격/이동 파라미터를 명시적으로 끕니다.
            enemyAnim.SetBool("isDead", true);
            enemyAnim.SetBool("isWalking", false);
            enemyAnim.SetBool("isAttacking", false);
            enemyAnim.ResetTrigger("Attack");

            // 어떤 트랜지션도 무시하고 즉시 "Die" 실행
            enemyAnim.Play("Die", 0, 0f);
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    // (FadeOutAndDestroy 및 ShowDamageText는 기존과 동일)
    void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null)
        {
            Transform canvasTransform = hpSlider.GetComponentInParent<Canvas>().transform;
            GameObject textObj = Instantiate(damageTextPrefab, canvasTransform, false);
            textObj.SetActive(true);
            textObj.transform.localPosition = new Vector3(100f, 200f, 0);
            textObj.GetComponent<DamageText>().Setup(damage);
        }
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return null;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        yield return new WaitForSeconds(1f);
        float fadeDuration = 0.5f;
        float currentTime = 0f;
        Color startColor = sr.color;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}