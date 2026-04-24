using UnityEngine;
using System.Collections;

public class MiniSpiderController : MonoBehaviour
{
    [Header("상태 및 타겟")]
    public Transform targetPlayer;

    [Header("절차적 다리 참조")]
    public ProceduralSpiderLeg[] frontLegs;
    public ProceduralSpiderLeg[] middleLegs;
    public float middleLegAttackRange = 3f;

    private SpiderBossState currentState = SpiderBossState.Idle;
    private EnemyHealth healthScript;
    private EnemyStats stats;
    private float lastAttackTime;

    private void Awake()
    {
        healthScript = GetComponent<EnemyHealth>();
        stats = GetComponent<EnemyStats>();
        targetPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (stats == null || stats.enemyData == null) return;

        if (healthScript != null && healthScript.IsDead)
        {
            currentState = SpiderBossState.Dead;
            return;
        }

        switch (currentState)
        {
            case SpiderBossState.Idle:
                if (targetPlayer != null) currentState = SpiderBossState.Chase;
                break;

            case SpiderBossState.Chase:
                if (targetPlayer == null) return;

                bool canFrontAttack = false;
                foreach (var leg in frontLegs)
                {
                    if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
                    {
                        canFrontAttack = true;
                        break;
                    }
                }

                bool canMiddleAttack = false;
                foreach (var leg in middleLegs)
                {
                    if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= middleLegAttackRange)
                    {
                        canMiddleAttack = true;
                        break;
                    }
                }

                if ((canFrontAttack || canMiddleAttack) && Time.time > lastAttackTime + stats.enemyData.attackCooldown)
                    StartCoroutine(MeleeAttackRoutine());
                else
                    transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, stats.moveSpeed * Time.deltaTime);
                break;
        }
    }

    private IEnumerator MeleeAttackRoutine()
    {
        currentState = SpiderBossState.Attack_Melee;
        lastAttackTime = Time.time;

        bool canFrontAttack = false;
        foreach (var leg in frontLegs)
        {
            if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
            {
                canFrontAttack = true;
                break;
            }
        }

        if (canFrontAttack)
        {
            ProceduralSpiderLeg closestLeg = null;
            float minDistance = float.MaxValue;
            foreach (var leg in frontLegs)
            {
                if (leg == null || leg.isDead) continue;
                float d = Vector2.Distance(leg.transform.position, targetPlayer.position);
                if (d < minDistance) { minDistance = d; closestLeg = leg; }
            }

            if (closestLeg != null)
            {
                float localX = transform.InverseTransformPoint(closestLeg.transform.position).x;
                int sweepDir = localX > 0 ? 1 : -1;
                closestLeg.PerformSweep(targetPlayer.position, sweepDir);
            }
        }

        foreach (var leg in middleLegs)
        {
            if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= middleLegAttackRange)
                leg.PerformSlam(leg.GetIdealPosition(), true, 2f, 2f);
        }

        yield return new WaitForSeconds(1.5f);
        currentState = SpiderBossState.Chase;
    }

    // EnemyHealth가 이 컴포넌트를 disable할 때 자동으로 코루틴 정리
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
