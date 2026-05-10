using UnityEngine;
using System.Collections;
using System.Linq; // FirstOrDefault 사용을 위해 추가
using Cainos.PixelArtTopDown_Basic;
using Game.Player;
using Game.Core; // ElementType, ISkillElementEnhancer 접근을 위해 추가

public class DashSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data (스크립터블 오브젝트 할당)")]
    public SkillData skillData;
    public Sprite Icon => skillData != null ? skillData.icon : null;

    private bool isDashing = false;
    private float lastDashTime = -999f;

    public float Cooldown => skillData != null ? skillData.cooldown : 1f;

    public float CooldownRemaining
    {
        get
        {
            float remain = (lastDashTime + Cooldown) - Time.time;
            return Mathf.Max(0f, remain);
        }
    }

    public bool TryUse(GameObject owner)
    {
        if (owner == null || skillData == null) return false;
        if (isDashing) return false;

        if (Time.time < lastDashTime + Cooldown)
        {
            Debug.Log("대쉬 쿨타임 중");
            return false;
        }

        var stats = owner.GetComponentInChildren<PlayerStats>();
        if (stats == null || !stats.SpendMP(skillData.skillManaCost))
        {
            Debug.Log("마나 부족으로 대쉬 불가");
            return false;
        }

        var rb = owner.GetComponentInChildren<Rigidbody2D>();
        var runner = owner.GetComponent<CoroutineRunner>();

        if (rb == null || runner == null) return false;

        // 1. 방향 결정
        Vector2 dir = GetDashDirection(owner);
        if (dir.sqrMagnitude < 0.0001f) return false;

        // 2. 현재 플레이어 원소 및 강화기 확인 (SwiftnessSkill 방식과 동일)
        var playerElement = owner.GetComponentInChildren<PlayerElement>();
        ElementType currentElement = playerElement != null ? playerElement.CurrentElement : ElementType.None;

        var enhancers = GetComponents<ISkillElementEnhancer>();
        ISkillElementEnhancer activeEnhancer = enhancers.FirstOrDefault(e => e.TargetElement == currentElement);

        // 3. 루틴 시작
        var controllers = owner.GetComponentsInChildren<TopDownCharacterController>(true);
        runner.StartCoroutine(DashRoutine(owner, rb, dir, controllers, activeEnhancer));

        lastDashTime = Time.time;
        return true;
    }

    private Vector2 GetDashDirection(GameObject owner)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 dashDir = ((Vector2)worldMousePos - (Vector2)owner.transform.position).normalized;

        if (dashDir.sqrMagnitude < 0.0001f)
        {
            var facing = owner.GetComponent<PlayerFacing>();
            if (facing != null && facing.LastFacingDir.sqrMagnitude > 0.0001f)
                return facing.LastFacingDir;

            return Vector2.right;
        }

        return dashDir;
    }

    private IEnumerator DashRoutine(GameObject owner, Rigidbody2D rb, Vector2 dir, TopDownCharacterController[] controllers, ISkillElementEnhancer enhancer)
    {
        isDashing = true;

        // 원소 효과 시작 (예: 불길 생성 시작, 물 정화 스택 쌓기 등)
        enhancer?.OnStart(owner);

        if (controllers != null)
            foreach (var c in controllers) if (c != null) c.enabled = false;

        rb.linearVelocity = Vector2.zero;
        // 속도 계산: 거리 / 시간
        float dashVelocity = skillData.dashDistance / Mathf.Max(0.0001f, skillData.dashTime);
        rb.linearVelocity = dir * dashVelocity;

        float elapsed = 0;
        while (elapsed < skillData.dashTime)
        {
            // 원소 효과 업데이트 (매 프레임 호출)
            enhancer?.OnUpdate(owner);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        if (controllers != null)
            foreach (var c in controllers) if (c != null) c.enabled = true;

        // 원소 효과 종료
        enhancer?.OnEnd(owner);

        isDashing = false;
    }
}