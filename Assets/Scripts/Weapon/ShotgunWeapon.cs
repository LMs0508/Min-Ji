using System.Collections;
using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("비주얼 & 애니메이션 오브젝트")]
    public GameObject attackVisualUp;
    public GameObject attackVisualDown;
    public GameObject attackVisualSide;
    
    public float attackDuration = 0.5f;

    [Header("발사 설정 (방향별 총구 위치)")]
    public Transform firePointUp;    // 위쪽 공격 시 총알 시작점
    public Transform firePointDown;  // 아래쪽 공격 시 총알 시작점
    public Transform firePointSide;  // 좌우 공격 시 총알 시작점

    private bool isAttacking = false;
    private Vector2 currentDirection;
    private float currentMultiplier = 1f;

    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        if (isAttacking) return;

        currentDirection = direction;
        currentMultiplier = multiplier; 

        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm != null)
        {
            wm.StartNewAttack(); // [추가] 새로운 공격 시작! 발사 권한 충전

            PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
            if (visualHandler != null)
            {
                visualHandler.isAttacking = true;
                visualHandler.PlayAttackAnimation(direction);
            }
            StartCoroutine(AttackRoutine(wm, direction));
        }
    }

  private IEnumerator AttackRoutine(WeaponManager wm, Vector2 direction)
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackDuration);

        // 공격이 끝나면 다시 모두 끕니다.
        if (attackVisualUp != null) attackVisualUp.SetActive(false);
        if (attackVisualDown != null) attackVisualDown.SetActive(false);
        if (attackVisualSide != null) attackVisualSide.SetActive(false);

        PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.isAttacking = false;
        }
        isAttacking = false;    
        }   

    // 애니메이션 이벤트에서 호출되는 실제 발사 로직
    public void FireBullet()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        // 몸통 반전 상태에 맞춰 무기 방향 동기화 (FirePoint 위치 보정)
        PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
        if (visualHandler != null && visualHandler.bodyAnimator != null)
        {
            float bodyScaleX = visualHandler.bodyAnimator.transform.localScale.x;
            Vector3 weaponScale = transform.localScale;
            weaponScale.x = Mathf.Abs(weaponScale.x) * (bodyScaleX < 0 ? -1f : 1f);
            transform.localScale = weaponScale;
        }
    
        // 1. 방향에 맞는 FirePoint 선택
        Transform activeFirePoint = transform; // 기본값
        
        if (Mathf.Abs(currentDirection.y) > Mathf.Abs(currentDirection.x))
        {
            if (currentDirection.y > 0) activeFirePoint = firePointUp;
            else activeFirePoint = firePointDown;
        }
        else
        {
            activeFirePoint = firePointSide;
        }

        // 2. [핵심 수정] 만약 선택된 FirePoint가 없다면 에러 로그를 찍고 함수를 종료하게 해서 원인을 찾습니다.
        if (activeFirePoint == null)
        {
            Debug.LogError($"<color=red>보시오! {name}의 FirePoint가 연결되지 않았습니다!</color>");
            return;
        }

        // 3. 총알 생성 위치 확정 (오브젝트의 실제 월드 좌표 사용)
        Vector3 spawnPosition = activeFirePoint.position;

        // --- 발사 로직 시작 ---
        float playerAtk = wm.GetCurrentPlayerAttack();
        float finalDamage = playerAtk * currentMultiplier; 
        float baseAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float[] spreads = { -15f, -5f, 5f, 15f };

        foreach (float offset in spreads)
        {
            // spawnPosition(FirePoint의 위치)에서 정확히 생성!
            GameObject bullet = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);

            float finalAngle = baseAngle + offset;
            Vector2 finalDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            bullet.GetComponent<Projectile>()?.Setup(data.projectileSpeed, data.attackRange, finalDamage, finalDir);
        }
    }
}