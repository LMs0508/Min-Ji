using System.Collections;
using UnityEngine;
using Game.Player;
using Game.Core;

public class LightBulletSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data (스크립터블 오브젝트 할당)")]
    public SkillData skillData;

    [Header("스킬 설정")]
    public float skillManaCost = 20f;
    public int shotCount = 6;
    public float timeBetweenShots = 0.2f;
    
    [Header("투사체 설정 (빛의 탄환 프리팹)")]
    public GameObject projectilePrefab;
    public GameObject castEffectPrefab; // 매 발사마다 총구에서 터질 이펙트 애니메이션
    public float projectileSpeed = 15f;
    public float projectileRange = 10f;

    public Sprite Icon => skillData != null ? skillData.icon : null;
    
    // 스크립터블 오브젝트에 쿨타임(6초)이 설정되어 있지 않을 경우 기본값 6초 사용
    public float Cooldown => skillData != null ? skillData.cooldown : 6f;
    public float CooldownRemaining => Mathf.Max(0f, (lastUsedTime + Cooldown) - Time.time);

    private float lastUsedTime = -999f;
    private bool isExecuting = false;

    public bool TryUse(GameObject owner)
    {
        if (owner == null || isExecuting || skillData == null) return false;
        if (Time.time < lastUsedTime + Cooldown) return false;

        // [조건 1] 현재 장착된 무기가 원거리(Ranged) 무기인지 확인합니다.
        var weaponManager = owner.GetComponentInChildren<WeaponManager>();
        if (weaponManager == null || weaponManager.currentWeapon == null || weaponManager.currentWeapon.weaponType != WeaponType.Ranged)
        {
            Debug.Log("<color=red>원거리(Ranged) 무기를 장착해야 빛의 탄환을 사용할 수 있습니다!</color>");
            return false;
        }

        var stats = owner.GetComponentInChildren<PlayerStats>();
        var runner = owner.GetComponent<CoroutineRunner>();

        if (stats == null || runner == null) return false;

        // [조건 2] 마나 소모 (20)
        if (!stats.SpendMP(skillManaCost)) return false;

        lastUsedTime = Time.time;
        runner.StartCoroutine(ExecuteLightBullet(owner, stats, weaponManager));
        return true;
    }

    private IEnumerator ExecuteLightBullet(GameObject owner, PlayerStats stats, WeaponManager weaponManager)
    {
        isExecuting = true;

        // [수정] 스킬 발사 중에는 일반 공격이 나가지 않도록 플래그를 설정합니다.
        if (weaponManager != null)
            weaponManager.IsSkillActive = true;

        var visualHandler = owner.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null) visualHandler.TriggerCombatMode();

        // [조건 3] 공격력의 100% 데미지
        float baseAttack = stats.Attack.Value;
        float damageMultiplier = skillData != null ? skillData.damageRatio : 1.0f;
        float finalDamage = baseAttack * damageMultiplier;

        // [조건 4] 6발 발사 연사루프
        for (int i = 0; i < shotCount; i++)
        {
            // 매 발사마다 마우스 위치를 갱신하여 플레이어가 조준을 바꿀 수 있게 합니다.
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 currentDir = (mousePos - owner.transform.position).normalized;

            // 캐릭터 시선 방향 전환 및 공격 애니메이션
            if (visualHandler != null && visualHandler.bodyAnimator != null)
            {
                visualHandler.bodyAnimator.SetFloat("MoveX", currentDir.x);
                visualHandler.bodyAnimator.SetFloat("MoveY", currentDir.y);
                
                Vector3 scale = visualHandler.bodyAnimator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (currentDir.x < 0 ? -1f : 1f);
                visualHandler.bodyAnimator.transform.localScale = scale;

                // [추가] 무기의 좌우 반전도 플레이어 방향에 맞춰 처리하여 FirePoint 위치를 보정합니다.
                WeaponBase weapon = owner.GetComponentInChildren<WeaponBase>();
                if (weapon != null)
                {
                    Vector3 weaponScale = weapon.transform.localScale;
                    weaponScale.x = Mathf.Abs(weaponScale.x) * (currentDir.x < 0 ? -1f : 1f);
                    weapon.transform.localScale = weaponScale;
                }
            }

            // [추가] 장착된 무기에서 마우스 방향에 맞는 FirePoint(총구)를 동적으로 찾습니다.
            WeaponBase activeWeapon = owner.GetComponentInChildren<WeaponBase>();
            if (activeWeapon != null)
            {
                activeWeapon.ExecuteAttack(currentDir, 1.0f);
            }

            Transform activeFirePoint = owner.transform;
            
            if (activeWeapon != null)
            {
                // [수정] 샷건/스나이퍼는 각 무기 스크립트에 직접 연결된 FirePoint를 사용하도록 변경합니다.
                // 이렇게 하면 복잡한 프리팹 구조와 상관없이 항상 정확한 위치에서 발사됩니다.
                var shotgun = activeWeapon as ShotgunWeapon;
                if (shotgun != null)
                {
                    if (Mathf.Abs(currentDir.y) > Mathf.Abs(currentDir.x))
                        activeFirePoint = (currentDir.y > 0) ? shotgun.firePointUp : shotgun.firePointDown;
                    else
                        activeFirePoint = shotgun.firePointSide;
                }
                var sniper = activeWeapon as SniperWeapon;
                if (sniper != null)
                {
                    if (Mathf.Abs(currentDir.y) > Mathf.Abs(currentDir.x))
                        activeFirePoint = (currentDir.y > 0) ? sniper.firePointUp : sniper.firePointDown;
                    else
                        activeFirePoint = sniper.firePointSide;
                }

                // 만약 위에서 FirePoint를 찾지 못했다면(샷건/스나이퍼가 아니거나, 변수가 할당 안됨)
                // 안전장치로 무기 자신의 위치를 사용합니다.
                if (activeFirePoint == owner.transform || activeFirePoint == null)
                {
                    activeFirePoint = activeWeapon.transform;
                }
            }

            // 최종 발사 위치 확정 (FirePoint가 아예 없으면 플레이어 몸통 약간 앞으로 설정)
            Vector3 spawnPos = activeFirePoint.position;
            if (activeFirePoint == owner.transform) spawnPos += (Vector3)currentDir * 0.5f; 
            
            // 매 발사마다 시전 위치에 발사 이펙트(애니메이션) 생성
            if (castEffectPrefab != null)
            {
                // [수정] 이펙트도 총구 위치에서 나가도록 spawnPos 사용
                GameObject effect = Instantiate(castEffectPrefab, spawnPos, Quaternion.Euler(0, 0, Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg));
                Destroy(effect, 0.5f); // 애니메이션이 끝날 즈음 자동 삭제
            }

            // 만약 전용 투사체가 설정 안 되어 있다면, 무기의 기본 투사체를 대신 사용 (유연성)
            GameObject projToUse = projectilePrefab != null ? projectilePrefab : weaponManager.currentWeapon.projectilePrefab;

            if (projToUse != null)
            {
                GameObject bullet = Instantiate(projToUse, spawnPos, Quaternion.identity);
                
                Projectile projScript = bullet.GetComponent<Projectile>();
                if (projScript != null)
                {
                    // 투사체 속도, 사거리, 데미지, 발사 방향 설정
                    projScript.Setup(projectileSpeed, projectileRange, finalDamage, currentDir);
                }
            }

            // [조건 5] 0.2초 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // [추가] 마지막 발사 애니메이션 이벤트가 뒤늦게 실행되어 일반 총알이 나가는 것을 막기 위해 충분히 대기합니다.
        yield return new WaitForSeconds(0.5f);

        // [수정] 스킬이 끝나면 플래그를 해제하여 다시 일반 공격이 가능하도록 합니다.
        if (weaponManager != null)
            weaponManager.IsSkillActive = false;

        isExecuting = false;
    }
}