using System.Collections;
using UnityEngine;
using Game.Player;
using Game.Core;

public class LightBulletSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data (스크립터블 오브젝트 할당)")]
    public SkillData skillData;

    [Header("스킬 설정")]
    public int shotCount = 6;
    public float timeBetweenShots = 0.1f;
    
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
        if (!stats.SpendMP(skillData.skillManaCost)) return false;

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

        var visualHandler = owner.GetComponentInChildren<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.TriggerCombatMode();
            // 스킬이 지속되는 동안 공격 모션으로 인해 캐릭터 방향이 고정되도록 합니다.
            visualHandler.isAttacking = true;
        }

        // [조건 3] 공격력의 100% 데미지
        float baseAttack = stats.Attack.Value;
        float damageMultiplier = skillData != null ? skillData.damageRatio : 1.0f;
        this.finalSkillDamage = baseAttack * damageMultiplier;

        // 스킬 시작 시의 마우스 방향으로 조준을 고정합니다.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        this.fireDirection = (mousePos - owner.transform.position).normalized;

        // 캐릭터 시선 방향을 고정된 조준 방향으로 설정합니다.
        if (visualHandler != null && visualHandler.bodyAnimator != null)
        {
            visualHandler.bodyAnimator.SetFloat("MoveX", this.fireDirection.x);
            visualHandler.bodyAnimator.SetFloat("MoveY", this.fireDirection.y);

            Vector3 scale = visualHandler.bodyAnimator.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (this.fireDirection.x < 0 ? -1f : 1f);
            visualHandler.bodyAnimator.transform.localScale = scale;

            // 무기의 좌우 반전도 플레이어 방향에 맞춰 처리하여 FirePoint 위치를 보정합니다.
            WeaponBase weapon = owner.GetComponentInChildren<WeaponBase>();
            if (weapon != null)
            {
                Vector3 weaponScale = weapon.transform.localScale;
                weaponScale.x = Mathf.Abs(weaponScale.x) * (this.fireDirection.x < 0 ? -1f : 1f);
                weapon.transform.localScale = weaponScale;
            }
        }

        // [조건 4] 6발 발사 연사루프
        for (int i = 0; i < shotCount; i++)
        {
            // [변경] 애니메이션 이벤트에 의존하지 않고, 직접 공격 애니메이션을 재생하고 발사 로직을 호출합니다.
            // 이렇게 하면 애니메이션이 0.1초마다 재시작되어 연사하는 것처럼 보입니다.
            if (visualHandler != null && visualHandler.bodyAnimator != null)
            {
                visualHandler.PlayAttackAnimation(this.fireDirection);
            }
            FireSkillBullet();

            // [조건 5] 0.1초 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // [추가] 스킬이 끝난 후 공격 방향 고정을 해제합니다.
        if (visualHandler != null)
        {
            visualHandler.isAttacking = false;
        }

        // 스킬 종료 및 정리
        weaponManager.IsSkillActive = false;
        isExecuting = false;
        this.skillOwner = null;
        this.ownerWeaponManager = null;
    }

    // 애니메이션 이벤트에 의해 호출될 실제 발사 로직
    private void FireSkillBullet()
    {
        if (skillOwner == null || ownerWeaponManager == null) return;

        // [수정] 현재 무기에서 직접 FirePoint를 가져오도록 로직을 개선합니다.
        WeaponBase activeWeapon = skillOwner.GetComponentInChildren<WeaponBase>();
        Transform activeFirePoint = (activeWeapon != null)
            ? activeWeapon.GetFirePoint(fireDirection)
            : skillOwner.transform;

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