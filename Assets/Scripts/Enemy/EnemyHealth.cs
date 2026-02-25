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

    [Header("VFX")]
    public GameObject damageTextPrefab;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();

        sr = GetComponentInChildren<SpriteRenderer>();
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


        currentHealth -= damage;


        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        ShowDamageText(damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void TakeDamage(float damage, Vector2 knockbackDir)
    {
        if (currentHealth <= 0) return;


        currentHealth -= damage;


        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        ShowDamageText(damage);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = knockbackDir * stats.knockbackForce;
        }

        StartCoroutine(HitFeedback());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

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

    private IEnumerator HitFeedback()
    {
        isHit = true;
        if (sr != null) sr.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        if (sr != null) sr.color = Color.white;
        isHit = false;
    }

    private void Die()
    {
        if (currentHealth < -999) return;
        currentHealth = -1000;

        if (hpSlider != null) hpSlider.gameObject.SetActive(false);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.SetBool("isWalking", false);
            anim.SetBool("isDead", true);
            anim.Play("Die", 0, 0f);
        }

        // 1. 기존 피격 피드백(빨간색 변함) 등이 꼬이지 않게 모든 코루틴 중단
        StopAllCoroutines();

        // 2. AI 및 물리 엔진 정지 (기존 코드와 동일)
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this && !(script is Animator)) script.enabled = false;
        }
        if (GetComponent<Rigidbody2D>() != null) GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<Collider2D>().enabled = false;

        // 3. 핵심: 서서히 사라지는 코루틴 시작!
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        yield return new WaitForSeconds(1f);

        float fadeDuration = 0.5f; // 사라지는 데 걸리는 시간
        float currentTime = 0f;
        Color startColor = sr.color;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;

            // 투명도(Alpha)를 1에서 0으로 선형 보간($Lerp$)합니다.
            float alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null; // 다음 프레임까지 대기
        }

        // 완전히 투명해지면 오브젝트 삭제
        Destroy(gameObject);
    }
}