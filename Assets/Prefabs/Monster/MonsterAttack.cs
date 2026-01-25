using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    private EnemyMover mover;
    private Animator anim;

    private Rigidbody2D rb;
    public Transform player;
    public float attackRange;
    public float attackCooldown;
    public float moveSpeed;
    private float lastAttackTime;

    void Start()
    {
        mover = GetComponentInParent<EnemyMover>();
        anim = GetComponent<Animator>();
        if (player == null )
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if(go != null )
            {
                player = go.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || mover == null || anim == null) return;

        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("AttackMotion") ||
                       anim.GetNextAnimatorStateInfo(0).IsName("AttackMotion");

        if (isAttacking)
        {// 공격 중이면 이동을 멈추게 하는 코드
            mover.Stop();
        }
        float distance = Vector2.Distance(transform.position, player.position);
        
        if(distance > attackRange)
        {
            if(!isAttacking)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                mover.Move(direction, moveSpeed);
            }
        }
        else
        {
            mover.Stop();
            if(Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }

    public void OnMonsterHit()
    {
        if (player == null) return;
        float currentDistance = Vector2.Distance(transform.position, player.position); if (currentDistance <= attackRange)
        {
            Debug.Log("HIT!");
        }
        else
        {
            Debug.Log("MISS!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 원의 색상을 빨간색으로 설정
        Gizmos.color = Color.red;

        // 현재 위치를 중심으로 attackRange 반지름만큼의 선으로 된 구(원)를 그림
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
