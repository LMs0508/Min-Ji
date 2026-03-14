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

    private JudgmentSmash baseSkill;

    public void OnStart(GameObject owner)
    {
        baseSkill = owner.GetComponentInChildren<JudgmentSmash>();

        if (baseSkill != null)
        {
            // [핵심 1] 프로젝트 창의 프리팹을 그대로 넣었을 경우를 대비해, 
            // 씬에 실체가 없으면 즉시 자동 생성(Instantiate)해서 자식으로 만듭니다.
            fireCharge = SafeInstantiate(fireCharge, this.transform);
            fireRise = SafeInstantiate(fireRise, this.transform);
            fireAir = SafeInstantiate(fireAir, this.transform);
            fireFall = SafeInstantiate(fireFall, this.transform);

            if (fireCharge) baseSkill.chargeVFX = fireCharge;
            if (fireRise) baseSkill.riseVFX = fireRise;
            if (fireAir) baseSkill.airVFX = fireAir;
            if (fireFall) baseSkill.fallVFX = fireFall;
        }
    }

    // 프리팹인지 씬 오브젝트인지 판별하고 안전하게 생성해주는 마법의 함수
    private GameObject SafeInstantiate(GameObject obj, Transform parent)
    {
        if (obj == null) return null;
        // scene.IsValid()가 false면 하이어라키(씬)에 없는 원본 프리팹이라는 뜻!
        if (!obj.scene.IsValid())
        {
            GameObject inst = Instantiate(obj, parent);
            inst.SetActive(false); // 생성 직후에는 일단 숨겨둠
            return inst;
        }
        return obj; // 이미 씬에 있는 자식 오브젝트면 그대로 사용
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
        if (baseSkill != null)
        {
            baseSkill.chargeVFX = baseSkill.defaultCharge;
            baseSkill.riseVFX = baseSkill.defaultRise;
            baseSkill.airVFX = baseSkill.defaultAir;
            baseSkill.fallVFX = baseSkill.defaultFall;
        }

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
                if (health != null)
                {
                    health.StartCoroutine(ApplyBurnEffect(health, burnDamage, burnDuration));
                }
            }
        }
    }

    private IEnumerator ApplyBurnEffect(EnemyHealth target, int damage, float duration)
    {
        float elapsed = 0;
        SpriteRenderer enemyRenderer = target.GetComponentInChildren<SpriteRenderer>();

        while (elapsed < duration && target != null && !target.IsDead)
        {
            target.TakeDamage(damage);
            if (enemyRenderer != null)
            {
                target.StartCoroutine(FlashRedEffect(enemyRenderer));
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