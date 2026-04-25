using Game.Player;
using UnityEngine;
using System.Collections;

public class MiniSpiderController : MonoBehaviour
{
    [Header("타겟")]
    public Transform targetPlayer;

    [Header("앞다리 공격 (ProceduralSpiderLeg 2개만)")]
    public ProceduralSpiderLeg[] frontLegs;

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

                bool canAttack = false;
                foreach (var leg in frontLegs)
                {
                    if (leg != null && !leg.isDead && Vector2.Distance(leg.transform.position, targetPlayer.position) <= stats.enemyData.attackRange)
                    {
                        canAttack = true;
                        break;
                    }
                }

                if (canAttack && Time.time > lastAttackTime + stats.enemyData.attackCooldown)
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

        yield return new WaitForSeconds(1.5f);
        currentState = SpiderBossState.Chase;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
