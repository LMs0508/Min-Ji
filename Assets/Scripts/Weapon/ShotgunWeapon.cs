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
            StartCoroutine(AttackRoutine(wm, direction));
        }
    }

  private IEnumerator AttackRoutine(WeaponManager wm, Vector2 direction)
    {
        isAttacking = true;
        wm.TogglePlayerVisuals(false);

        // 혹시 켜져있을지 모르는 모든 이펙트를 끕니다.
        if (attackVisualUp != null) attackVisualUp.SetActive(false);
        if (attackVisualDown != null) attackVisualDown.SetActive(false);
        if (attackVisualSide != null) attackVisualSide.SetActive(false);

        // 마우스 방향에 따라 알맞은 오브젝트 1개를 플레이어 위치로 옮긴 후 켭니다.
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            if (direction.y > 0)
            {
                if (attackVisualUp != null)
                {
                    attackVisualUp.transform.position = wm.transform.position; // [복구] 위치 동기화
                    attackVisualUp.SetActive(true);
                }
            }
            else
            {
                if (attackVisualDown != null)
                {
                    attackVisualDown.transform.position = wm.transform.position; // [복구] 위치 동기화
                    attackVisualDown.SetActive(true);
                }
            }
        }
        else
        {
            if (attackVisualSide != null)
            {
                attackVisualSide.transform.position = wm.transform.position; // [복구] 위치 동기화
                attackVisualSide.SetActive(true);
                
                // 좌우 오브젝트일 때는 마우스 위치(왼쪽/오른쪽)에 맞춰 플립(Scale X) 적용
                Vector3 scale = attackVisualSide.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1f : 1f);
                attackVisualSide.transform.localScale = scale;
            }
        }

        yield return new WaitForSeconds(attackDuration);

        // 공격이 끝나면 다시 모두 끕니다.
        if (attackVisualUp != null) attackVisualUp.SetActive(false);
        if (attackVisualDown != null) attackVisualDown.SetActive(false);
        if (attackVisualSide != null) attackVisualSide.SetActive(false);

        wm.TogglePlayerVisuals(true);
        isAttacking = false;
    }

    // 애니메이션 이벤트에서 호출되는 실제 발사 로직
    public void FireBullet()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        float playerAtk = wm.GetCurrentPlayerAttack();
        float finalDamage = playerAtk * currentMultiplier; 

        float baseAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float[] spreads = { -15f, -5f, 5f, 15f };

        // =========================================================
        // [핵심 추가] 쏜 방향에 맞춰 알맞은 FirePoint를 가져옵니다!
        // =========================================================
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

        Vector3 spawnPosition = (activeFirePoint != null) ? activeFirePoint.position : transform.position;

        foreach (float offset in spreads)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);

            float finalAngle = baseAngle + offset;
            Vector2 finalDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            bullet.GetComponent<Projectile>()?.Setup(data.projectileSpeed, data.attackRange, finalDamage, finalDir);
        }
    }
}