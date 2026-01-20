using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{
    [Header("데이터 소스")]
    public EnemyData enemyData; // 유니티 인스펙터에서 EnemyData를 꽂아주는 곳

    [Header("비주얼 설정")]
    public Transform visuals; // 플립용 변수

    // 밑에 코드들은 EnemyData파일에서 받아올 실제 값들을 저장할 변수들
    protected int currentHealth;
    protected float moveSpeed;
    protected float detectionRange;
    protected float stopDistance;
    protected float knockbackForce;

    // 배회 관련 설정들도 EnemyData파일에서 가져옴
    protected float wanderSpeed;
    protected float wanderDuration;
    protected float waitDuration;

    protected Transform player; // 자식 클래스에서도 쓸 수 있게 protected로 함.
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected bool isChasing = false; // 플레이어를 쫒고 있는가?
    protected bool isHit = false; // 타격된 상태인가? --> 이때 이동을 멈추게 할 거임

    // 밑에 코드들은 배회할 때 쓸 변수들
    private Vector2 wanderDirection;
    private float wanderTimer;
    private bool isWaiting = false;
    // 몬스터 스프라이트가 좌우반전될 때 히트박스도 같이 움직이도록 하기 위한 변수설정.
    protected bool isFacingRight = true; 

    protected virtual void Awake() // virtual를 써야 자식클래스에서 활용 가능하다고 함
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (visuals != null)
        {
            sr = visuals.GetComponent<SpriteRenderer>();
        }

        // 받아온 EnemyData가 있다면 그 수치들을 이 몬스터의 값으로 사용함
        if (enemyData != null )
        {
            moveSpeed = enemyData.moveSpeed;
            detectionRange = enemyData.detectionRange;
            stopDistance = enemyData.stopDistance;
            knockbackForce = enemyData.knockbackForce;

            wanderSpeed = enemyData.wanderSpeed;
            wanderDuration = enemyData.wanderDuration;
            waitDuration = enemyData.waitDuration;

            currentHealth = enemyData.maxHealth; //시작할 때 현재 체력을 최대 체력으로 설정함
        }
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
        if (player == null || isHit)
            return; // 플레이어가 없거나 맞아서 넉백 중일 땐 리턴

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

    public virtual void TakeDamage(int damage) // 피격함수임. 플레이어가 몬스터를 때릴 때 이 함수가 호출될 예정
    {
        if (currentHealth <= 0)
            return; // 이미 죽었다면 무시

        currentHealth -= damage;
        Debug.Log($"{enemyData.enemyName} 피격, 남은 체력: {currentHealth}");

        StartCoroutine(HitFeedback()); // 피격시 빨간색으로 표시

        // 넉백 효과 계산해주는 것
        Vector2 knockbackDir = (transform.position - player.position).normalized;
        rb.linearVelocity = knockbackDir * knockbackForce;

        // 체력 다 떨어졌는지 확인
        if (currentHealth <= 0)
            Die();
    }

    // 코루틴 --> 피격 시 빨간색으로 변하게 하는 것
    protected IEnumerator HitFeedback()
    {
        isHit = true; // 피격 중엔 몬스터 움직일 수 없게 할 것
        sr.color = Color.red;

        yield return new WaitForSeconds(0.2f); // 이것을 0.2초 동안 유지

        sr.color = Color.white; // 다시 원래 색으로 복구
        isHit = false; // 경직 풀어줌
    }

    protected virtual void Die()
    {
        Destroy(gameObject); // 몬스터 오브젝트 삭제
        // 나중에 아이템 드롭 코드를 여기다 넣어도 될듯
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
        float randomY = Random.Range(-1f, 1f); // 무작위 Y방향
        wanderDirection = new Vector2(randomX, randomY).normalized;
    }

    protected virtual void FlipSprite()
    {
        // 속도가 거의 0일때는 방향 전환하지 않는걸로
        if (Mathf.Abs(rb.linearVelocity.x) < 0.05f)
            return;
        // 왼쪽으로 가고 있고, 현재 오른쪽을 보고 있으면 플립
        if(rb.linearVelocity.x < 0 && isFacingRight)
        {
            Flip();
        }
        // 이건 오른쪽으로 가고 있을때
        else if (rb.linearVelocity.x > 0 && !isFacingRight)
        {
            Flip();
        }
    }

    protected void Flip()
    {
        // 보는 방향 상태를 반대로 바꿈
        isFacingRight = !isFacingRight;

        if (visuals != null)
        {
            // 현재 Scale을 가져옴
            Vector3 localScale = visuals.localScale;

            // x축 크기에 -1 곱해서 반전시키는 것
            localScale.x *= -1;

            // 이제 그 반전된 Scale을 다시 적용
            visuals.localScale = localScale;
        } // 이러면 이제 몬스터 프리팹에서 자식에 있는 것들만 좌우반전됨 --> 나중에 ui나 다른 거 추가되면 그것까지 뒤집힐 수 있어서 그거 방지용으로 이렇게 만듬
    }
}
