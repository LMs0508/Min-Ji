using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    public override void ExecuteAttack(Vector2 direction)
    {
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float[] spreads = { -30f, -10f, 10f, 30f }; // 4방향 각도

        foreach (float offset in spreads)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);

            float finalAngle = baseAngle + offset;
            Vector2 finalDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

            bullet.GetComponent<Projectile>()?.Setup(data.projectileSpeed, data.attackRange, data.attackDamage, finalDir);
        }

        Debug.Log("샷건 4발 발사!");
    }
}