using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyMover mover;
    private EnemyHealth health;
    private Transform player;

    private float wanderTimer;
    private Vector2 wanderDirection;
    private bool isWaiting = false;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        mover = GetComponent<EnemyMover>();
        health = GetComponent<EnemyHealth>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null || health.isHit)
            return;
        float distance = Vector2.Distance(transform.position, player.position);
        
        if(distance <= stats.detectionRange)
        {
            HandleChasing(distance);
        }
        else
        {
            HandlePatrol();
        }
    }

    private void HandleChasing(float distance)
    {
        if(distance > stats.stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            mover.Move(dir, stats.moveSpeed);
        }
        else // 일정거리에 도달하면 멈춤
        {
            mover.Stop();
        }
    }

    private void HandlePatrol()
    {
        wanderTimer -= Time.deltaTime;
        if(wanderTimer <=0)
        {
            isWaiting = !isWaiting;
            if(!isWaiting)
            {
                wanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                wanderTimer = stats.wanderDuration;
            }
            else
            {
                mover.Stop();
                wanderTimer = stats.waitDuration;
            }
        }

        if(!isWaiting)
        {
            mover.Move(wanderDirection, stats.moveSpeed);
        }
    }

  
}
