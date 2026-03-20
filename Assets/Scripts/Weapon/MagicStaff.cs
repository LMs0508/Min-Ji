using UnityEngine;
using System.Collections;

public class MagicStaff : WeaponBase
{
    [Header("발사 설정")]
    public Transform firePoint; // 마법구가 생성될 지팡이 끝부분
    public float attackDuration = 0.5f; // 공격 방향 고정 시간

    private bool isAttacking = false;
    private Vector2 currentDirection;
    private float currentMultiplier = 1f;

    private PlayerAttackInput attackInput;
    private PlayerVisualHandler visualHandler;

    private void Start()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm != null)
        {
            attackInput = wm.GetComponent<PlayerAttackInput>();
            visualHandler = transform.root.GetComponentInChildren<PlayerVisualHandler>();

            // [핵심] 차징 이벤트 구독! (A키를 누르고 있을 때 호출됨)
            if (attackInput != null)
            {
                attackInput.OnChargeChanged += HandleChargingAnimation;
            }
        }
    }

    private void OnDestroy()
    {
        // 무기를 바꾸거나 파괴될 때 이벤트 구독 해제
        if (attackInput != null)
        {
            attackInput.OnChargeChanged -= HandleChargingAnimation;
        }
    }

    // 차징 비율(0.0 ~ 1.0)에 따라 애니메이터의 IsCharging을 켜고 끕니다.
    private void HandleChargingAnimation(float ratio)
    {
        if (visualHandler != null && visualHandler.bodyAnimator != null)
        {
            bool isCharging = ratio > 0f;
            visualHandler.bodyAnimator.SetBool("IsCharging", isCharging);
        }
    }

    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        if (isAttacking) return;

        currentDirection = direction;
        currentMultiplier = multiplier;

        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm != null)
        {
            wm.StartNewAttack(); // 발사 권한 충전

            if (visualHandler != null)
            {
                visualHandler.isAttacking = true;
                visualHandler.PlayAttackAnimation(direction);
            }

            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackDuration);

        if (visualHandler != null) visualHandler.isAttacking = false;
        isAttacking = false;
    }

    // 애니메이션 이벤트(TriggerFire)에 의해 호출되는 실제 발사 로직
    public void FireBullet()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        float currentAtk = wm.GetCurrentPlayerAttack();
        float currentMag = wm.GetCurrentPlayerMagic();

        float rawDamage = (currentAtk * 0.5f + currentMag * 0.5f) * currentMultiplier;
        int finalDamage = Mathf.RoundToInt(rawDamage);

        Transform spawnPoint = (firePoint != null) ? firePoint : transform;

        if (data.projectilePrefab != null)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, spawnPoint.position, Quaternion.identity);
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(data.projectileSpeed, data.attackRange, finalDamage, currentDirection);
            }
        }

        if (currentMultiplier >= 1.5f)
            Debug.Log($"<color=cyan><b>[FULL CHARGE]</b></color> 마법 지팡이 발사! 데미지: {finalDamage}");
        else
            Debug.Log($"마법 지팡이 발사! 데미지: {finalDamage} (차징: {currentMultiplier:F1}x)");
    }
}