using UnityEngine;

public class MagicStaff : WeaponBase
{
    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();

        // 공격력과 마력을 모두 WeaponManager를 통해 가져옵니다.
        float currentAtk = (wm != null) ? wm.GetCurrentPlayerAttack() : 0;
        float currentMag = (wm != null) ? wm.GetCurrentPlayerMagic() : 0; // 이 부분이 핵심입니다!

        float rawDamage = (currentAtk * 0.5f + currentMag * 0.5f) * multiplier;
        int finalDamage = Mathf.RoundToInt(rawDamage);

        // 4. 투사체 발사 (마법구 등)
        if (data.projectilePrefab != null)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Setup(data.projectileSpeed, data.attackRange, finalDamage, direction);
            }
        }

        Debug.Log($"마법 지팡이 발사! 데미지: {finalDamage} (차징: {multiplier:F1}x)");
    }
}