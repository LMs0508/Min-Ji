using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    private Rigidbody2D rb;
    public Transform visuals;
    private bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction * speed;
        FlipSprite(direction.x);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void FlipSprite(float xInput)
    {
        // 속도가 거의 0일때는 방향 전환하지 않는걸로
        if (Mathf.Abs(xInput) < 0.05f)
            return;
        if ((xInput<0 && isFacingRight) || (xInput >0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            if(visuals != null)
            {
                Vector3 scale = visuals.localScale;
                scale.x *= -1;
                visuals.localScale = scale;
            }
        }
    }
}
