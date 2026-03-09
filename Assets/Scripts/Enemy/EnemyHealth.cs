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

        // [핵심 추가] 몬스터가 죽는 시점에 QuestManager에게 사냥 성공 보고
        if (QuestManager.Instance != null && stats != null && stats.enemyData != null)
        {
            // MonsterHunt 타입으로, EnemyData에 적힌 이름을 ID로 전달하여 카운트를 올립니다.
            QuestManager.Instance.ProgressQuest(QuestType.MonsterHunt, stats.enemyData.enemyName, 1);
            Debug.Log($"퀘스트 보고됨: {stats.enemyData.enemyName} 사냥 완료");
        }

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

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(FadeOutAndDestroy());
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