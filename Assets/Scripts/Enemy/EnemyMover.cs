using UnityEngine;
using System.Collections;

public class EnemyMover : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    public Transform visuals;
    private bool isFacingRight = true;
    private EnemyHealth health;

    private bool isStunned = false;
    public bool IsStunned => isStunned;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        health = GetComponent<EnemyHealth>();
    }

    void Update()
    {
        if (health != null && health.IsDead) return;

        if (isStunned && rb.linearVelocity.magnitude < 0.1f)
        {
            Debug.Log($"<color=yellow>[경고]</color> {gameObject.name}가 넉백 힘을 받았으나, 누군가에 의해 정지되었습니다!");
        }

        if (anim != null)
        {
            float speed = isStunned ? 0 : rb.linearVelocity.magnitude;
            anim.SetBool("isWalking", speed > 0.1f);
        }
    }

    public void Move(Vector2 direction, float speed)
    {
        if (isStunned || (health != null && health.IsDead)) return;

        rb.linearVelocity = direction * speed;
        FlipSprite(direction.x);
    }

    public void Stop()
    {
        if (isStunned || (health != null && health.IsDead)) return;
        rb.linearVelocity = Vector2.zero;
    }

    public void ApplyStun(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StunRoutine(duration));
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        if (anim != null)
        {
            anim.Play("Idle", 0, 0f);
            anim.SetBool("isWalking", false);
        }

        yield return new WaitForSeconds(duration);

        isStunned = false;
    }

    public void LookAt(Vector2 direction)
    {
        if (isStunned || (health != null && health.IsDead)) return;
        FlipSprite(direction.x);
    }

    private void FlipSprite(float xInput)
    {
        if (health != null && health.IsDead) return;
        if (Mathf.Abs(xInput) < 0.05f) return;

        bool movingRight = xInput > 0;

        if (movingRight != isFacingRight)
        {
            isFacingRight = movingRight;

            if (visuals != null)
            {
                Vector3 scale = visuals.localScale;
                scale.x = Mathf.Abs(scale.x) * (movingRight ? 1 : -1);
                visuals.localScale = scale;
            }
        }
    }
}
