using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    public override void ExecuteAttack(Vector2 direction)
    {
        // 1. WeaponManager 참조 가져오기
        // (보통 무기 프리팹은 플레이어의 자식이므로 GetComponentInParent가 효율적입니다)
        WeaponManager wm = GetComponentInParent<WeaponManager>();

        float playerAtk = 0;
        if (wm != null)
        {
            playerAtk = wm.GetCurrentPlayerAttack();
        }

        // 2. 최종 데미지 계산 (공격력의 80%)
        float finalDamage = playerAtk * 0.8f;

        // 3. 샷건 발사 로직
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float[] spreads = { -15f, -5f, 5f, 15f };

        foreach (float offset in spreads)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);

            float finalAngle = baseAngle + offset;
            Vector2 finalDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            // 4. 총알에 계산된 데미지 전달
            bullet.GetComponent<Projectile>()?.Setup(data.projectileSpeed, data.attackRange, finalDamage, finalDir);
        }
    }
}