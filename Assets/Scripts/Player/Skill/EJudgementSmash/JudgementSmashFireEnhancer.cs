using UnityEngine;
using Game.Core;
using System.Collections;

public class JudgementSmashFireEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Fire;

    [Header("ȭ�Ӽ� ���� ����Ʈ (�ν����Ϳ��� ���� ��������)")]
    public GameObject fireCharge;
    public GameObject fireRise;
    public GameObject fireAir;
    public GameObject fireFall;

    [Header("�Ҳ� ���� ����")]
    public GameObject fireTrailPrefab;
    public GameObject groundFirePrefab;
    public float explosionRadius = 3f;

    [Header("ȭ��(Burn) ����")]
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
            // [�ٽ� 1] ������Ʈ â�� �������� �״��� �־��� ���츦 ������, 
            // ���� ��ü�� ������ ���� �ڵ� ����(Instantiate)�ؼ� �ڽ����� �����ϴ�.
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

    // ���������� �� ������Ʈ���� �Ǻ��ϰ� �����ϰ� �������ִ� ������ �Լ�
    private GameObject SafeInstantiate(GameObject obj, Transform parent)
    {
        if (obj == null) return null;
        // scene.IsValid()�� false�� ���̾���Ű(��)�� ���� ���� �������̶��� ��!
        if (!obj.scene.IsValid())
        {
            GameObject inst = Instantiate(obj, parent);
            inst.SetActive(false); // ���� ���Ŀ��� �ϴ� ���ܵ�
            return inst;
        }
        return obj; // �̹� ���� �ִ� �ڽ� ������Ʈ�� �״��� ����
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
                EnemyHealth health = hit.GetComponentInParent<EnemyHealth>();
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