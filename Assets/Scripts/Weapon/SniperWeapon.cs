using UnityEngine;

public class SniperWeapon : WeaponBase
{
    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        // 1. WeaponManager를 통해 플레이어의 현재 최종 공격력 가져오기
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        float playerAtk = (wm != null) ? wm.GetCurrentPlayerAttack() : 0;

        // 2. 데미지 계산
        // 저격총 기본 데미지 * 차징 배율(1.0 ~ 1.5배)
        // 풀 차징 시 최대 공격력의 150% 데미지가 들어갑니다.
        float finalDamage = playerAtk * 1f*multiplier;

        // 3. 총알 소환
        if (data.projectilePrefab != null)
        {
            GameObject bullet = Instantiate(data.projectilePrefab, transform.position, Quaternion.identity);

            // 4. 총알 설정 (방향, 속도, 사거리, 계산된 데미지)
            Projectile projectileScript = bullet.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                // Sniper.asset에 설정한 긴 사거리와 빠른 속도를 사용합니다.
                projectileScript.Setup(data.projectileSpeed, data.attackRange, finalDamage, direction);
            }
        }

        // 차징 정도에 따른 로그 출력 (타격감 연출용)
        if (multiplier >= 1.5f)
        {
            Debug.Log("<color=cyan><b>[FULL CHARGE]</b></color> 저격총 발사! 데미지: " + finalDamage);
        }
        else
        {
            Debug.Log($"저격총 발사! 데미지: {finalDamage} (배율: {multiplier:F1}x)");
        }
    }
}