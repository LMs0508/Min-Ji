using UnityEngine;
using Game.Core;
using System.Collections;

public class JudgementSmashFireEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Fire;

    [Header("화속성 오버라이드 컨트롤러 (4개)")]
    public AnimatorOverrideController chargeOverride;
    public AnimatorOverrideController riseOverride;
    public AnimatorOverrideController airOverride;
    public AnimatorOverrideController fallOverride;

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
            ApplyOverride(skillBase.chargeVFX, chargeOverride);
            ApplyOverride(skillBase.riseVFX, riseOverride);
            ApplyOverride(skillBase.airVFX, airOverride);
            ApplyOverride(skillBase.fallVFX, fallOverride);
        }
    }

    private void ApplyOverride(GameObject vfxObj, AnimatorOverrideController overrideCtrl)
    {
        if (vfxObj == null || overrideCtrl == null) return;

        Animator anim = vfxObj.GetComponent<Animator>();
        if (anim == null) anim = vfxObj.GetComponentInChildren<Animator>();

        if (anim != null)
        {
            anim.runtimeAnimatorController = overrideCtrl;
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
                if (health != null)
                {
                    StartCoroutine(ApplyBurnEffect(health));
                }
            }
        }
    }

    private IEnumerator ApplyBurnEffect(EnemyHealth target)
    {
        float elapsed = 0;
        while (elapsed < burnDuration && target != null && !target.IsDead)
        {
            target.TakeDamage(burnDamage); 
            elapsed += 1.0f;
            yield return new WaitForSeconds(1.0f);
        }
    }
}