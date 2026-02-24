using UnityEngine;
using Game.Player;

public class WeaponCharge : MonoBehaviour
{
    private PlayerStats playerStats;
    private bool isCharging = false;

    [Header("Direction Settings")]
    private Vector2 lastMoveDirection = Vector2.right; // 기본값은 오른쪽

    [Header("Settings")]
    public float maxChargeTime = 2.0f;
    public float chargeTime = 0f;
    public LayerMask enemyLayer;

    [Header("Visuals")]
    public GameObject rangeIndicator;
    public Vector2 baseAttackSize = new Vector2(2f, 1.5f);

    void Awake()
    {
        playerStats = GetComponentInChildren<PlayerStats>();
        if (rangeIndicator != null) rangeIndicator.SetActive(false);
    }

    void Update()
    {
        // 1. 마지막으로 누른 방향키 저장 (공격 중이 아닐 때만 업데이트 가능)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            // 대각선 이동 시 길이를 1로 맞춤 (Normalize)
            lastMoveDirection = new Vector2(h, v).normalized;
        }

        // 2. 차징 로직 (Z키)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isCharging = true;
            chargeTime = 0f;
            if (rangeIndicator != null) rangeIndicator.SetActive(true);
        }

        if (isCharging && Input.GetKey(KeyCode.Z))
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);
            UpdateRangeVisual();
        }

        if (Input.GetKeyUp(KeyCode.Z) && isCharging)
        {
            FireSkill();
            isCharging = false;
            if (rangeIndicator != null) rangeIndicator.SetActive(false);
        }
    }

    void UpdateRangeVisual()
    {
        float ratio = chargeTime / maxChargeTime;
        float visualMult = 1.0f;

        if (ratio >= 1.0f) visualMult = 1.5f;
        else if (ratio >= 0.75f) visualMult = 1.3f;

        if (rangeIndicator != null)
        {
            rangeIndicator.transform.localScale = new Vector3(visualMult, visualMult, 1f);

            // 시각적 표시기도 플레이어가 바라보는 방향으로 위치 이동
            float offsetDistance = (baseAttackSize.x * visualMult) / 2.5f;
            rangeIndicator.transform.localPosition = (Vector3)lastMoveDirection * offsetDistance;
        }
    }

    void FireSkill()
    {
        float ratio = (chargeTime / maxChargeTime) * 100f;
        float damageMult = 0.1f;
        float rangeMult = 1.0f;

        if (ratio >= 100f) { damageMult = 0.8f; rangeMult = 1.5f; }
        else if (ratio >= 75f) { damageMult = 0.5f; rangeMult = 1.3f; }
        else if (ratio >= 50f) { damageMult = 0.4f; }
        else if (ratio >= 25f) { damageMult = 0.2f; }

        ExecuteAttack(damageMult, rangeMult);
    }

    void ExecuteAttack(float damageMult, float rangeMult)
    {
        int finalDamage = Mathf.RoundToInt(playerStats.Attack.Value * damageMult);

        // --- 방향 반영 메커니즘 ---
        // 1. 공격 위치 계산: 플레이어 위치 + (바라보는 방향 * 공격 범위의 절반 정도의 거리)
        float offsetDistance = (baseAttackSize.x * rangeMult) / 2f;
        Vector2 attackPos = (Vector2)transform.position + (lastMoveDirection * offsetDistance);

        // 2. 공격 범위 크기
        Vector2 attackSize = baseAttackSize * rangeMult;

        // 3. 공격 각도 계산 (박스를 회전시켜야 함)
        // lastMoveDirection의 각도를 구해서 OverlapBox에 넣어줍니다.
        float angle = Mathf.Atan2(lastMoveDirection.y, lastMoveDirection.x) * Mathf.Rad2Deg;

        // 4. 범위 내 적 감지 (각도 적용)
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPos, attackSize, angle, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.GetComponent<EnemyHealth>() != null)
            {
                // 적의 위치 - 나의 위치 = 밀려날 방향
                Vector2 knockback = (enemy.transform.position - transform.position).normalized;
                enemy.GetComponent<EnemyHealth>().TakeDamage(finalDamage, knockback);
            }
        }
    }

    // 에디터에서 범위를 확인하기 위한 Gizmo (바라보는 방향으로 그려짐)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float offsetDistance = baseAttackSize.x / 2f;
        Vector2 attackPos = (Vector2)transform.position + (lastMoveDirection * offsetDistance);

        // 회전을 시각적으로 확인하기 위해 각도 계산
        float angle = Mathf.Atan2(lastMoveDirection.y, lastMoveDirection.x) * Mathf.Rad2Deg;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(attackPos, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, baseAttackSize);
    }
}