using UnityEngine;
using Game.Core;
using System.Collections;

public class JudgementSmashFireEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Fire;

    [Header("화속성 전용 이펙트 (인스펙터에서 직접 넣으세요)")]
    public GameObject fireCharge;
    public GameObject fireRise;
    public GameObject fireAir;
    public GameObject fireFall;

    [Header("불꽃 연출 설정")]
    public GameObject fireTrailPrefab;
    public GameObject groundFirePrefab;
    public float explosionRadius = 3f;

    [Header("화상(Burn) 설정")]
    public int burnDamage = 5;
    public float burnDuration = 3f;

    private float trailTimer;
    public float trailInterval = 0.05f;

    public void OnStart(GameObject owner)
    {
        JudgmentSmash skillBase = owner.GetComponentInChildren<JudgmentSmash>();
        if (skillBase != null)
        {
            // [핵심] 메인 스크립트가 사용할 이펙트를 화속성용으로 강제 교체합니다.
            if (fireCharge) skillBase.chargeVFX = fireCharge;
            if (fireRise) skillBase.riseVFX = fireRise;
            if (fireAir) skillBase.airVFX = fireAir;
            if (fireFall) skillBase.fallVFX = fireFall;
        }
    }

    public void OnUpdate(GameObject owner)
    {
        trailTimer += Time.deltaTime;
        if (trailTimer >= trailInterval)
        {
            if (fireTrailPrefab != null)
                Instantiate(fireTrailPrefab, owner.transform.position, Quaternion.identity);
            trailTimer = 0f;
        }
    }

    public void OnEnd(GameObject owner)
    {
        Vector3 landingPos = owner.transform.position;
        if (groundFirePrefab != null)
        {
            GameObject fireField = Instantiate(groundFirePrefab, landingPos, Quaternion.identity);
            Destroy(fireField, 3.0f);
        }
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(landingPos, explosionRadius);
        foreach (var hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth health = hit.GetComponent<EnemyHealth>();
                if (health != null) StartCoroutine(ApplyBurnEffect(health));
            }
        }
    }

    private IEnumerator ApplyBurnEffect(EnemyHealth target)
    {
        float elapsed = 0;
        SpriteRenderer enemyRenderer = target.GetComponentInChildren<SpriteRenderer>();
        while (elapsed < burnDuration && target != null && !target.IsDead)
        {
            target.TakeDamage(burnDamage);
            if (enemyRenderer != null)
            {
                StartCoroutine(FlashRedEffect(enemyRenderer));
            }
            elapsed += 1.0f;
            yield return new WaitForSeconds(1.0f);
        }
    }
    private IEnumerator FlashRedEffect(SpriteRenderer renderer)
    {
        if (renderer == null) yield break;

        Color originalColor = renderer.color;
        renderer.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (renderer != null)
        {
            renderer.color = originalColor;
        }
    }
}