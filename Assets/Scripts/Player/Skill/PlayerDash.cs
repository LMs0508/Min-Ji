using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerDash : MonoBehaviour
{
    [Header("돌진 설정")]
    public float dashDistance;    
    public float dashTime;      
    public float dashCooldown;

    private Rigidbody2D rb;
    private bool isDashing = false;
    private float lastDashTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing) return;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            if (Time.time >= lastDashTime + dashCooldown)
            {
                Vector2 dir = GetDashDirection();
                StartCoroutine(DashRoutine(dir));
            }
        }
    }

    private Vector2 GetDashDirection()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(x, y).normalized;

        // 아무 방향키도 안 눌렀다면 캐릭터가 바라보는 방향으로 돌진
        if (inputDir == Vector2.zero)
        {
            inputDir = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
        }
        return inputDir;
    }

    private IEnumerator DashRoutine(Vector2 dir)
    {
        isDashing = true;
        var controller = GetComponent<TopDownCharacterController>(); // 이동 코드 잠시 정지
        if (controller != null) controller.enabled = false;

        lastDashTime = Time.time;

        // 원거리 고블린이 멈추듯, 대시 직전 속도 초기화
        rb.linearVelocity = Vector2.zero;

        float dashVelocity = dashDistance / dashTime;
        rb.linearVelocity = dir * dashVelocity;

        yield return new WaitForSeconds(dashTime);

        rb.linearVelocity = Vector2.zero;

        if (controller != null) controller.enabled = true; // 이동 코드 다시 실행

        isDashing = false;
    }
}