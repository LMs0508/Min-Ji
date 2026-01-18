using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("기본 셋팅")]
    public float moveSpeed = 2f;
    public float detectionRange = 8f; // 이건 플레이어를 발견하는 거리
    public float stopDistance = 1.2f; // 멈추는 거리(근접 기준임) 원거리일 때는 숫자를 5정도로 키우면 될 듯

    [Header("배회 셋팅")]
    public float wanderSpeed = 1f; //배회 속도
    public float wanderDuration = 2f; // 한 방향인 동안 이동하는 시간
    public float waitDuration = 1.5f; // 이동 후 대기하는 시간

    protected Transform player; // 자식 클래스에서도 쓸 수 있게 protected로 함.
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected bool isChasing = false; // 플레이어를 쫒고 있는가?

    // 밑에 코드들은 배회할 때 쓸 변수들
    private Vector2 wanderDirection;
    private float wanderTimer;
    private bool isWaiting = false;

    protected virtual void Awake() // virtual를 써야 자식클래스에서 활용 가능하다고 함
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        // 밑에 코드는 태그를 통해 몬스터가 플레이어를 찾기 위해서임
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        SetNewWanderDirection(); // 시작할 때 첫 이동방향 설정
    }

    protected virtual void Update() // 이것도 자식이 쓸 수 있게 하기 위해 virtual씀
    {
        if (player == null)
            return; // 플레이어 없으면 리턴
        float distance = Vector2.Distance(transform.position, player.position);
        // 플레이어 거리를 계산하기 위한 코드
        if (distance <= detectionRange) //거리가 감지범위 안에 들어왔다면
        {
            isChasing = true; // 추격 시작
            if (distance > stopDistance) // 멈출 거리보단 멀다면 추격
            {
                Move(); // Move함수를 활용
            }
            else // 멈출 거리에 도달했다면 멈추는 거죵
            {
                StopMoving(); // StopMoving 함수 활용
            }
        }
        else // 아직 감지범위 안이 아니라면
        {
            isChasing = false; // 추격 false상태 그대로
            Patrol(); // 아직 발견못했으니 순찰하는 함수로
        }
            FlipSprite();
    }

    protected virtual void Move() // 움직임 함수
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    protected virtual void StopMoving() // 멈추는 거죵
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected virtual void Patrol() // 순찰 함수
    {
        wanderTimer -= Time.deltaTime; // 이게 0이 될때마다 몬스터 상태가 움직(방향)이거나 정지해 있거나 한 상태로 바뀜
        
        if (wanderTimer <=0)
        {
            if(!isWaiting) // 기다리는 상태가 아닌 즉, 이동 중이었다면 대기 상태로 전환
            {
                isWaiting = true;
                StopMoving();
                wanderTimer = waitDuration; // 대기시간으로 설정
            }
            else // 기다리는 상태였다면 이제 배회해야지
            {
                isWaiting = false;
                SetNewWanderDirection();
                wanderTimer = wanderDuration; // 배회시간으로 설정
            }
        }
        if (!isWaiting) // 배회중일 때 속도
        {
            rb.linearVelocity = wanderDirection * wanderSpeed;
        }
    }

    private void SetNewWanderDirection() // 아까는 시작할 때 방향을 정했다면 이건 그 이후 방향 설정 함수
    {
        float randomX = Random.Range(-1f, 1f); // 무작위 X방향
        float randomY = Random.Range(-1f. 1f); // 무작위 Y방향
        wanderDirection = new Vector2(randomX, randomY).normalized;
    }

    protected virtual void FlipSprite()
    {
        if(Mathf.Abs(rb.linear))
    }
}
