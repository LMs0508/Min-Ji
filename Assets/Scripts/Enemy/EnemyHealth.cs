using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{// 몬스터 피격판정, 체력 감소 등을 관리하기 위한 파일
    private EnemyStats stats;
    private SpriteRenderer sr;
    public int currentHealth;
    public bool isHit = false;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        sr = GetComponentInChildren<SpriteRenderer>(); // 자식인 Visuals에 스프라이트 있는 경우 대비
    }

    void Start()
    {
        currentHealth = stats.maxHealth;
    }

    public void TakeDamage(int damage, Vector2 knockbackDir)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;
        // 넉백 적용
        GetComponent<Rigidbody2D>().linearVelocity = knockbackDir * stats.knockbackForce;
        StartCoroutine(HitFeedback());
        if (currentHealth <= 0)
            Die();
    }

    // 코루틴 --> 피격 시 빨간색으로 변하게 하는 것
    private IEnumerator HitFeedback()
    {
        isHit = true; // 피격 중엔 몬스터 움직일 수 없게 할 것
        sr.color = Color.red;

        yield return new WaitForSeconds(0.2f); // 이것을 0.2초 동안 유지

        sr.color = Color.white; // 다시 원래 색으로 복구
        isHit = false; // 경직 풀어줌
    }

    private void Die()
    {
        Destroy(gameObject); // 몬스터 오브젝트 삭제
    }
}
