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

    // 애니메이션 이벤트 발사를 위한 임시 데이터
    private GameObject skillOwner;
    private WeaponManager ownerWeaponManager;
    private Vector2 fireDirection;
    private float finalSkillDamage;

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
        weaponManager.IsSkillActive = true;

        // 이벤트 핸들러에서 사용할 정보 저장
        this.skillOwner = owner;
        this.ownerWeaponManager = weaponManager;

        // 스킬 발사 이벤트 구독
        weaponManager.OnSkillFireRequest += FireSkillBullet;

        var visualHandler = owner.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null) visualHandler.TriggerCombatMode();

        // [조건 3] 공격력의 100% 데미지
        float baseAttack = stats.Attack.Value;
        float damageMultiplier = skillData != null ? skillData.damageRatio : 1.0f;
        this.finalSkillDamage = baseAttack * damageMultiplier;

        // [조건 4] 6발 발사 연사루프
        for (int i = 0; i < shotCount; i++)
        {
            // 매 발사마다 마우스 위치를 갱신하여 플레이어가 조준을 바꿀 수 있게 합니다.
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            this.fireDirection = (mousePos - owner.transform.position).normalized;

            // 캐릭터 시선 방향 전환 및 공격 애니메이션
            if (visualHandler != null && visualHandler.bodyAnimator != null)
            {
                visualHandler.bodyAnimator.SetFloat("MoveX", this.fireDirection.x);
                visualHandler.bodyAnimator.SetFloat("MoveY", this.fireDirection.y);
                
                Vector3 scale = visualHandler.bodyAnimator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (this.fireDirection.x < 0 ? -1f : 1f);
                visualHandler.bodyAnimator.transform.localScale = scale;

                // [추가] 무기의 좌우 반전도 플레이어 방향에 맞춰 처리하여 FirePoint 위치를 보정합니다.
                WeaponBase weapon = owner.GetComponentInChildren<WeaponBase>();
                if (weapon != null)
                {
                    Vector3 weaponScale = weapon.transform.localScale;
                    weaponScale.x = Mathf.Abs(weaponScale.x) * (this.fireDirection.x < 0 ? -1f : 1f);
                    weapon.transform.localScale = weaponScale;
                }
            }

            // 애니메이션을 재생시켜 이벤트가 'FireSkillBullet'을 호출하도록 합니다.
            WeaponBase activeWeapon = owner.GetComponentInChildren<WeaponBase>();
            if (activeWeapon != null)
            {
                activeWeapon.ExecuteAttack(this.fireDirection, 1.0f);
            }

            // [조건 5] 0.2초 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }

        yield return new WaitForSeconds(0.5f);

        // 스킬 종료 및 정리
        weaponManager.OnSkillFireRequest -= FireSkillBullet;
        weaponManager.IsSkillActive = false;
        isExecuting = false;
        this.skillOwner = null;
        this.ownerWeaponManager = null;
    }

    // 애니메이션 이벤트에 의해 호출될 실제 발사 로직
    private void FireSkillBullet()
    {
        if (skillOwner == null || ownerWeaponManager == null) return;

        Transform activeFirePoint = skillOwner.transform;
        WeaponBase activeWeapon = skillOwner.GetComponentInChildren<WeaponBase>();
        if (activeWeapon != null)
        {
            var shotgun = activeWeapon as ShotgunWeapon;
            if (shotgun != null) activeFirePoint = (Mathf.Abs(fireDirection.y) > Mathf.Abs(fireDirection.x)) ? ((fireDirection.y > 0) ? shotgun.firePointUp : shotgun.firePointDown) : shotgun.firePointSide;

            var sniper = activeWeapon as SniperWeapon;
            if (sniper != null) activeFirePoint = (Mathf.Abs(fireDirection.y) > Mathf.Abs(fireDirection.x)) ? ((fireDirection.y > 0) ? sniper.firePointUp : sniper.firePointDown) : sniper.firePointSide;

            if (activeFirePoint == skillOwner.transform || activeFirePoint == null) activeFirePoint = activeWeapon.transform;
        }

        Vector3 spawnPos = activeFirePoint.position;
        if (activeFirePoint == skillOwner.transform) spawnPos += (Vector3)fireDirection * 0.5f;

        if (castEffectPrefab != null)
        {
            GameObject effect = Instantiate(castEffectPrefab, spawnPos, Quaternion.Euler(0, 0, Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg));
            Destroy(effect, 0.5f);
        }

        GameObject projToUse = projectilePrefab != null ? projectilePrefab : ownerWeaponManager.currentWeapon.projectilePrefab;
        if (projToUse != null)
        {
            GameObject bullet = Instantiate(projToUse, spawnPos, Quaternion.identity);
            Projectile projScript = bullet.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.Setup(projectileSpeed, projectileRange, finalSkillDamage, fireDirection);
            }
        }
    }
}