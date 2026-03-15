using System.Collections;
using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("비주얼 & 애니메이션")]
    public GameObject attackVisualObject;
    public float attackDuration = 0.5f;

    [Header("발사 설정")]
    public Transform firePoint;

    private bool isAttacking = false;

    private Vector2 currentDirection;

    public override void ExecuteAttack(Vector2 direction)
    {
        if (isAttacking) return;

        currentDirection = direction;

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

        if (attackVisualObject != null)
        {
            attackVisualObject.SetActive(true);
            attackVisualObject.transform.position = wm.transform.position;

            Vector3 scale = attackVisualObject.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1f : 1f);
            attackVisualObject.transform.localScale = scale;

            Animator anim = attackVisualObject.GetComponent<Animator>();
            if (anim != null) anim.Play("Attack", 0, 0f);
        }

        // 발사는 이벤트가 알아서 해주니까, 여기서는 그냥 애니메이션이 끝날 때까지만 얌전히 기다립니다.
        yield return new WaitForSeconds(attackDuration);

        if (attackVisualObject != null)
        {
            attackVisualObject.SetActive(false);
        }

        wm.TogglePlayerVisuals(true);
        isAttacking = false;
    }

    public void FireBullet()
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        float playerAtk = wm.GetCurrentPlayerAttack();
        float finalDamage = playerAtk * 0.8f;

        // 저장해뒀던 마우스 방향을 꺼내 씁니다.
        float baseAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float[] spreads = { -15f, -5f, 5f, 15f };

        Vector3 spawnPosition = (firePoint != null) ? firePoint.position : transform.position;

        foreach (float offset in spreads)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, spawnPosition, Quaternion.identity);

            float finalAngle = baseAngle + offset;
            Vector2 finalDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            bullet.GetComponent<Projectile>()?.Setup(data.projectileSpeed, data.attackRange, finalDamage, finalDir);
        }
    }
}