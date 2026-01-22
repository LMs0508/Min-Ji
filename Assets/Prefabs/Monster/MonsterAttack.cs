using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    private Animator anim;
    public Transform player;
    public float attackRange;
    public float attackCooldown;
    private float lastAttackTime;

    void Start()
    {
        anim = GetComponent<Animator>();
        if(player == null )
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
        if (player == null)
            return;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }

    public void OnMonsterHit()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            Debug.Log("플레이어에게 데미지를 입혔습니다.");
        }
    }
}
