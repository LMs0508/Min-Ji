using UnityEngine;
using System.Collections;

public class SniperWeapon : WeaponBase
{
    [Header("발사 설정 (방향별 총구 위치)")]
    public Transform firePointUp;
    public Transform firePointDown;
    public Transform firePointSide;

    [Header("공격 설정")]
    public float attackDuration = 0.5f; // 공격 중 방향 고정 시간

    private bool isAttacking = false;
    private Vector2 currentDirection;
    private float currentMultiplier = 1f;

    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        if (isAttacking) return;

        // 1. 현재 공격 정보 저장 (FireBullet에서 사용)
        currentDirection = direction;
        currentMultiplier = multiplier;

        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm != null)
        {
            wm.StartNewAttack(); // [추가] 새로운 공격 시작! 발사 권한 충전

            // 2. 비주얼 핸들러를 통한 애니메이션 재생 및 방향 잠금
            PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
            if (visualHandler != null)
            {
                visualHandler.isAttacking = true; // 방향 고정 자물쇠 잠금
                visualHandler.PlayAttackAnimation(direction); // 애니메이션 재생
            }

            StartCoroutine(AttackRoutine(wm));
        }
    }

    private IEnumerator AttackRoutine(WeaponManager wm)
    {
        isAttacking = true;

        // 애니메이션 재생 시간 동안 대기
        yield return new WaitForSeconds(attackDuration);

        // 방향 잠금 해제
        PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.isAttacking = false;
        }

        isAttacking = false;
    }

    // [중요] 애니메이션 이벤트(TriggerFire)에 의해 호출되는 실제 발사 로직
    public void FireBullet()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        // 1. 몸통 반전 상태에 맞춰 무기 방향 동기화 (FirePoint 위치 보정)
        PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
        if (visualHandler != null && visualHandler.bodyAnimator != null)
        {
            float bodyScaleX = visualHandler.bodyAnimator.transform.localScale.x;
            Vector3 weaponScale = transform.localScale;
            weaponScale.x = Mathf.Abs(weaponScale.x) * (bodyScaleX < 0 ? -1f : 1f);
            transform.localScale = weaponScale;
        }

        // 2. 방향에 맞는 FirePoint 선택
        Transform activeFirePoint = transform;
        if (Mathf.Abs(currentDirection.y) > Mathf.Abs(currentDirection.x))
        {
            if (currentDirection.y > 0) activeFirePoint = firePointUp;
            else activeFirePoint = firePointDown;
        }
        else
        {
            activeFirePoint = firePointSide;
        }

        // 3. 데미지 및 총알 생성 위치 확정
        float playerAtk = wm.GetCurrentPlayerAttack();
        float finalDamage = playerAtk * currentMultiplier;
        Vector3 spawnPosition = (activeFirePoint != null) ? activeFirePoint.position : transform.position;

        // 4. 총알 생성 및 설정
        if (data.projectilePrefab != null)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(data.projectileSpeed, data.attackRange, finalDamage, currentDirection);
            }
        }

        // 로그 출력
        if (currentMultiplier >= 1.5f)
            Debug.Log("<color=cyan><b>[FULL CHARGE]</b></color> 저격총 발사! 데미지: " + finalDamage);
        else
            Debug.Log($"저격총 발사! 데미지: {finalDamage} (배율: {currentMultiplier:F1}x)");
    }
}