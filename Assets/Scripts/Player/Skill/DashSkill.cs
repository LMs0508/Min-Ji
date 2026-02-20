using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;
using Game.Player;

public class DashSkill : MonoBehaviour, ISkill
{
    [Header("UI")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

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
        Debug.Log($"AXIS H={Input.GetAxisRaw("Horizontal")} V={Input.GetAxisRaw("Vertical")}");
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
        if (dir.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("Dash dir is zero, cancel dash");
            return false;
        }

        var controllers = owner.GetComponentsInChildren<TopDownCharacterController>(true);
        runner.StartCoroutine(DashRoutine(owner, rb, dir, controllers));

        lastDashTime = Time.time;
        return true;
    }

    private Vector2 GetDashDirection(GameObject owner)
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(x, y);

        if (input.sqrMagnitude > 0.0001f)
        {
            input.Normalize();
            return input;
        }

        var facing = owner.GetComponent<PlayerFacing>();
        if (facing != null && facing.LastFacingDir.sqrMagnitude > 0.0001f)
            return facing.LastFacingDir;

        // 최후: 오른쪽
        return Vector2.right;
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