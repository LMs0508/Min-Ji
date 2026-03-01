using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossStatueAI : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyHealth health;
    private Animator anim;
    private Transform player;

    [Header("페이즈 설정")]
    public bool isPhase2 = false;
    public float phase2Threshold = 0.5f; // 체력 50% 이하 시 2페이즈
    public GameObject phase2Visual;    // 눈 6개 뜬 모습 오브젝트

    [Header("공격 설정")]
    public float attackCooldown = 3f;
    private float lastAttackTime;
    private bool isAttacking = false;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        anim = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (health.currentHealth <= 0 || isAttacking) return;

        // 페이즈 체크
        CheckPhase();

        float distance = Vector2.Distance(transform.position, player.position);

        // 공격 사거리 내에 있고 쿨타임이 끝났을 때
        if (distance <= stats.enemyData.attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformRandomAttack());
        }
    }

    private void CheckPhase()
    {
        if (!isPhase2 && health.currentHealth <= stats.maxHealth * phase2Threshold)
        {
            isPhase2 = true;
            anim.SetTrigger("Phase2Transition");
            if (phase2Visual != null) phase2Visual.SetActive(true);
            Debug.Log("보스 2페이즈 돌입! 괴물 석상이 눈을 뜹니다.");
        }
    }

    private IEnumerator PerformRandomAttack()
    {
        isAttacking = true;

        // 1페이즈는 1~3번 공격, 2페이즈는 1~5번 공격 중 랜덤
        int maxAttackIdx = isPhase2 ? 6 : 4;
        int attackChoice = Random.Range(1, maxAttackIdx);

        anim.SetTrigger("Attack" + attackChoice);

        // 애니메이션 길이에 맞춰 대기 (예: 1.5초)
        yield return new WaitForSeconds(1.5f);

        lastAttackTime = Time.time;
        isAttacking = false;
    }
}