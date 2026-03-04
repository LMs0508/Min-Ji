using UnityEngine;
using System.Collections;

public class EnemyEncounter : MonoBehaviour
{
    [Header("조우 연출 설정")]
    public GameObject encounterAnimationObject;
    public GameObject effectObject;
    public float encounterDuration = 1.2f;

    [Header("추격 설정")]
    public float minChaseDuration = 1.0f;
    private float chaseTimer = 0f;

    private EnemyMover mover;
    private EnemyStats stats;
    private EnemyHealth health; // [추가] 피격 상태 확인용

    private bool hasEncountered = false;
    public bool IsEncountering { get; private set; }
    public bool IsChasing { get; private set; }
    public bool IsForceChasing => chaseTimer > 0;

    void Awake()
    {
        mover = GetComponentInParent<EnemyMover>();
        stats = GetComponentInParent<EnemyStats>();
        health = GetComponentInParent<EnemyHealth>(); // [할당]

        if (encounterAnimationObject != null) encounterAnimationObject.SetActive(false);
        if (effectObject != null) effectObject.SetActive(false);
    }

    void Update()
    {
        if (chaseTimer > 0) chaseTimer -= Time.deltaTime;
    }

    public bool CheckEncounter(float distanceToPlayer)
    {
        if (hasEncountered || IsEncountering) return false;

        if (distanceToPlayer <= stats.enemyData.detectionRange)
        {
            // [체크] 피격 중(isHit)이라면 조우를 시작하지 않습니다.
            if (health != null && health.isHit) return false;

            StartCoroutine(EncounterRoutine());
            return true;
        }
        return false;
    }

    private IEnumerator EncounterRoutine()
    {
        // 1. 피격 연출(isHit)이 끝날 때까지 프레임 단위로 대기
        while (health != null && health.isHit)
        {
            yield return null;
        }

        // 2. 피격이 끝나면 비로소 조우 시작
        IsEncountering = true;
        hasEncountered = true;
        if (mover != null) mover.Stop();

        // 본체 애니메이터 끄기
        Animator mainAnim = GetComponent<Animator>();
        if (mainAnim != null) mainAnim.enabled = false;

        // 모든 스프라이트 숨기기 (본체 포함)
        SpriteRenderer[] allSrs = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var s in allSrs)
        {
            bool isEncounterPart = encounterAnimationObject != null && s.transform.IsChildOf(encounterAnimationObject.transform);
            if (!isEncounterPart) s.enabled = false;
        }

        // 조우 오브젝트 켜기
        if (encounterAnimationObject != null) encounterAnimationObject.SetActive(true);

        yield return new WaitForSeconds(encounterDuration);

        // [체크] 기다리는 동안 죽었다면 여기서 멈춤
        if (health != null && health.IsDead) yield break;

        // 3. 연출 종료 및 "강제" 복구
        if (encounterAnimationObject != null) encounterAnimationObject.SetActive(false);

        if (mainAnim != null) mainAnim.enabled = true;
        foreach (var s in allSrs) s.enabled = true; // 여기서 모든 스프라이트를 다시 켭니다.

        IsEncountering = false;
        IsChasing = true;
        chaseTimer = minChaseDuration;
    }

    public void ResetEncounter()
    {
        if (hasEncountered && chaseTimer <= 0)
        {
            hasEncountered = false;
            IsChasing = false;
        }
    }
}