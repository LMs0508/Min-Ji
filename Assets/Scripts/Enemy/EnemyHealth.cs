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

    protected virtual void Awake()
    {
        stats = GetComponent<EnemyStats>();
        sr = GetComponentInChildren<SpriteRenderer>();
        enemyAnim = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        currentHealth = stats.maxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = stats.maxHealth;
            hpSlider.value = currentHealth;
            
            // [핵심 수정] hpSlider가 비어있지 않을 때만 Canvas를 찾도록 안전하게 감쌉니다.
            Canvas hpCanvas = hpSlider.GetComponentInParent<Canvas>();
            if (hpCanvas != null)
            {
                hpCanvas.worldCamera = Camera.main;
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;
        
        // [추가] 거미 보스 기믹 처리: 모든 다리가 부서지지 않았다면 본체는 무적
        SpiderBossController bossCtrl = GetComponent<SpiderBossController>();
        if (bossCtrl != null && bossCtrl.IsInvincible())
        {
            ShowDamageText(0);
            return;
        }

        HandleDamage(damage, Vector2.zero);
    }

    public virtual void TakeDamage(float damage, Vector2 knockbackDir)
    {
        if (currentHealth <= 0) return;

        // [추가] 넉백 스킬로 타격 시에도 동일하게 무적 처리
        SpiderBossController bossCtrl = GetComponent<SpiderBossController>();
        if (bossCtrl != null && bossCtrl.IsInvincible())
        {
            // 다리가 아직 살아있으므로 본체 데미지 무효화
            ShowDamageText(0);
            return;
        }

        HandleDamage(damage, knockbackDir);
    }

    // [개선] 공통 데미지 처리 로직
    private void HandleDamage(float damage, Vector2 knockbackDir)
    {
        currentHealth -= damage;
        if (hpSlider != null) hpSlider.value = currentHealth;
        ShowDamageText(damage);

        // [핵심 수정] 조우 스크립트가 아예 없거나(보스 등), 조우 중이 아닐 때 피격 연출을 무조건 실행합니다!
        EnemyEncounter encounter = GetComponentInChildren<EnemyEncounter>();
        if (encounter == null || !encounter.IsEncountering)
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
        SpriteRenderer[] allSrs = GetComponentsInChildren<SpriteRenderer>();
        if (defenceVisual != null)
        {
            if (sr != null) sr.enabled = false;
            defenceVisual.SetActive(true); // 피격 스프라이트 오브젝트 활성화

            // [핵심] 피격 오브젝트 내부의 모든 스프라이트를 붉게 물들임
            SpriteRenderer[] hitSrs = defenceVisual.GetComponentsInChildren<SpriteRenderer>();
            foreach (var s in hitSrs)
            {
                s.color = Color.red;
            }
        }
        else
        {
            // 방어막(defenceVisual)이 없다면, 보스 몸통과 다리 모두를 직접 붉게 만듭니다!
            foreach (var s in allSrs) 
            {
                // [수정] 이름이 "Shadow"인 그림자 오브젝트는 색상 변경에서 제외합니다!
                if (s.gameObject.name != "Shadow") s.color = Color.red;
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
        if (defenceVisual != null)
        {
            if (sr != null) sr.enabled = true;
            // 색상을 다시 하얗게 돌려놓고 비활성화 (다음에 또 써야 하니까요)
            SpriteRenderer[] hitSrs = defenceVisual.GetComponentsInChildren<SpriteRenderer>();
            foreach (var s in hitSrs)
            {
                s.color = Color.white;
            }
            defenceVisual.SetActive(false);
        }
        else
        {
            foreach (var s in allSrs) 
            {
                if (s.gameObject.name != "Shadow") s.color = Color.white;
            }
        }

        isHit = false;
    }

    // --- 이하 Die 및 기타 로직은 동일하지만 defenceVisual 처리 추가 ---

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // [핵심 추가] 몬스터가 죽는 시점에 QuestManager에게 사냥 성공 보고
        if (QuestManager.Instance != null && stats != null && stats.enemyData != null)
        {
            // MonsterHunt 타입으로, EnemyData에 적힌 이름을 ID로 전달하여 카운트를 올립니다.
            QuestManager.Instance.ProgressQuest(QuestType.MonsterHunt, stats.enemyData.enemyName, 1);
            Debug.Log($"퀘스트 보고됨: {stats.enemyData.enemyName} 사냥 완료");
        }

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
        // [수정] 부모뿐만 아니라 자식(Body, 다리 등)에 있는 모든 콜리더를 찾아 꺼줍니다.
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (var c in cols)
        {
            c.enabled = false;
        }

        // 4. 모든 스크립트(Mover, Attack 등) 비활성화
        // [핵심] 여기서 스크립트가 꺼져야 Update에서의 방향 전환과 공격 명령이 멈춥니다.
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script == this) continue;
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
            SafeSetBool(enemyAnim, "isDead", true);
            SafeSetBool(enemyAnim, "isWalking", false);
            SafeSetBool(enemyAnim, "isAttacking", false);
            SafeResetTrigger(enemyAnim, "Attack");

            // 어떤 트랜지션도 무시하고 즉시 "Die" 실행
            if (enemyAnim.HasState(0, Animator.StringToHash("Die")))
            {
                enemyAnim.Play("Die", 0, 0f);
            }
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    // [핵심 추가] 데미지 텍스트를 담을 전용 '월드 스페이스' 캔버스를 가져오거나 생성합니다.
    public Canvas GetDamageTextCanvas()
    {
        // 1. 기존 체력바 캔버스가 월드 스페이스라면 그것을 사용 (가장 안전)
        if (hpSlider != null)
        {
            Canvas hpCanvas = hpSlider.GetComponentInParent<Canvas>();
            if (hpCanvas != null && hpCanvas.renderMode == RenderMode.WorldSpace)
                return hpCanvas;
        }

        // 2. 데미지 텍스트 전용으로 만든 캔버스가 이미 있는지 확인
        Transform existingCanvas = transform.Find("DamageTextCanvas");
        if (existingCanvas != null)
            return existingCanvas.GetComponent<Canvas>();

        // 2. 없으면 새로 생성 (UI 렌더링 누락을 막기 위해 RectTransform 명시 추가)
        GameObject canvasObj = new GameObject("DamageTextCanvas", typeof(RectTransform));
        canvasObj.transform.SetParent(this.transform, false);
        canvasObj.transform.localPosition = Vector3.zero;
        
        // [해결] 카메라 설정에 따라 텍스트가 안 보이는 현상을 막기 위해 UI 레이어 명시적 지정
        canvasObj.layer = LayerMask.NameToLayer("UI");

        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.WorldSpace;
        newCanvas.worldCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        newCanvas.sortingOrder = 999;

        // [수정] 캔버스의 스케일은 가장 기본값인 0.01로 안전하게 고정합니다. (유니티 렌더링 버그 방지)
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        return newCanvas;
    }

    // (FadeOutAndDestroy 및 ShowDamageText는 기존과 동일)
    protected virtual void ShowDamageText(float damage)
    {
        if (damageTextPrefab != null)
        {
                Canvas targetCanvas = GetDamageTextCanvas();
                if (targetCanvas != null)
                {
                    GameObject textObj = Instantiate(damageTextPrefab, targetCanvas.transform, false);
                    textObj.SetActive(true);
                    
                    // [수정] 하드코딩 제거. 타겟 위치로 이동 후, 프리팹에서 설정한 로컬 오프셋 값을 더해줍니다.
                    textObj.transform.position = transform.position;
                    textObj.transform.localPosition += damageTextPrefab.transform.localPosition;
                    
                    RectTransform rect = textObj.GetComponent<RectTransform>();
                    RectTransform prefabRect = damageTextPrefab.GetComponent<RectTransform>();
                    if (rect != null && prefabRect != null) {
                        rect.sizeDelta = prefabRect.sizeDelta; // 텍스트 상자 크기 적용
                        
                        // [핵심 해결] 캔버스가 보스 크기에 의해 뻥튀기되더라도 텍스트 크기를 역산하여 일정하게 맞춥니다.
                        Vector3 canvasWorldScale = targetCanvas.transform.lossyScale;
                        Vector3 baseScale = prefabRect.localScale * 0.01f;

                        float finalX = canvasWorldScale.x != 0 ? baseScale.x / canvasWorldScale.x : prefabRect.localScale.x;
                        float finalY = canvasWorldScale.y != 0 ? baseScale.y / canvasWorldScale.y : prefabRect.localScale.y;
                        float finalZ = canvasWorldScale.z != 0 ? baseScale.z / canvasWorldScale.z : prefabRect.localScale.z;

                        rect.localScale = new Vector3(finalX, finalY, finalZ);
                    }
                    
                    textObj.GetComponent<DamageText>().Setup(damage);
                }
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

    private void SafeSetBool(Animator anim, string paramName, bool value)
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(paramName, value);
                break;
            }
        }
    }

    private void SafeResetTrigger(Animator anim, string paramName)
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.ResetTrigger(paramName);
                break;
            }
        }
    }
}