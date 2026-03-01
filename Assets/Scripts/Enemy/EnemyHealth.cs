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
        if (isDead) return;
        isDead = true;

        currentHealth = -1000;
        if (hpSlider != null) hpSlider.gameObject.SetActive(false);

        StopAllCoroutines();
        if (sr != null) sr.color = Color.white;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.Normal;
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                    anim.ResetTrigger(param.name);
            }
  

            anim.SetBool("isWalking", false);
            anim.SetBool("isDead", true);

            // 강제 재생
            anim.Play("Die", 0, 0f);
        }

        // 다른 스크립트 비활성화
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script == null || script == this) continue;
            script.enabled = false;
        }

        // [수정] 여기서 즉시 끄지 않고, 아래 코루틴에서 처리합니다.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        // [추가] 애니메이션이 시작될 수 있도록 한 프레임 쉽니다.
        yield return null;

        // [추가] 이제 안전하게 물리 시뮬레이션을 끕니다.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(1f); // 시체가 유지되는 시간

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