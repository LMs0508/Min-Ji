using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;
using Game.Player;

public class DashSkill : MonoBehaviour, ISkill
{
    public float dashDistance = 3f;
    public float dashTime = 0.12f;
    public float dashCooldown = 1f;
    public float skillManaCost = 10f;

    private bool isDashing = false;
    private float lastDashTime = -999f;

    public float Cooldown => dashCooldown;

    public float CooldownRemaining
    {
        get
        {
            float remain = (lastDashTime + dashCooldown) - Time.time;
            return Mathf.Max(0f, remain);
        }
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null) return false;
        if (isDashing) return false;

        if (Time.time < lastDashTime + dashCooldown)
        {
            Debug.Log("쿨타임");
            return false;
        }

        var rb = owner.GetComponentInChildren<Rigidbody2D>();
        var runner = owner.GetComponent<CoroutineRunner>();
        var stats = owner.GetComponentInChildren<PlayerStats>();

        if (rb == null || runner == null) return false;
        if (stats == null || !stats.SpendMP(skillManaCost))
        {
            Debug.Log("마나 부족");
            return false;
        }

        Vector2 dir = GetDashDirection(owner);

        var controllers = owner.GetComponentsInChildren<TopDownCharacterController>(true);
        runner.StartCoroutine(DashRoutine(owner, rb, dir, controllers));

        lastDashTime = Time.time;
        return true;
    }

    private Vector2 GetDashDirection(GameObject owner)
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(x, y).normalized;

        if (inputDir == Vector2.zero)
            inputDir = (owner.transform.localScale.x > 0) ? Vector2.right : Vector2.left;

        return inputDir;
    }

    private IEnumerator DashRoutine(GameObject owner, Rigidbody2D rb, Vector2 dir, TopDownCharacterController[] controllers)
    {
        isDashing = true;

        if (controllers != null)
            foreach (var c in controllers) if (c != null) c.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = dir * (dashDistance / Mathf.Max(0.0001f, dashTime));

        yield return new WaitForSeconds(dashTime);

        rb.linearVelocity = Vector2.zero;

        if (controllers != null)
            foreach (var c in controllers) if (c != null) c.enabled = true;

        isDashing = false;
    }
}