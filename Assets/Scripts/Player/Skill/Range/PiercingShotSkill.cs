using System.Collections;
using UnityEngine;
using Game.Player;
using Game.Core;

public class PiercingShotSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data (스크립터블 오브젝트 할당)")]
    public SkillData skillData;

    [Header("스킬 설정")] 
    public float damageRatio = 1.2f; // 공격력의 120%
    
    [Header("투사체 설정 (관통형 투사체 프리팹)")]
    [Tooltip("레이저 형태의 길쭉한 프리팹을 할당하면 좋습니다.")]
    public GameObject projectilePrefab;
    public GameObject castEffectPrefab;
    public float projectileSpeed = 25f; // 레이저처럼 빠르게 날아가도록 기본 속도를 조금 높게 설정
    public float projectileRange = 15f;

    public Sprite Icon => skillData != null ? skillData.icon : null;
    
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

        // [조건 1] 현재 장착된 무기가 원거리(Ranged) 무기인지 확인
        var weaponManager = owner.GetComponentInChildren<WeaponManager>();
        if (weaponManager == null || weaponManager.currentWeapon == null || weaponManager.currentWeapon.weaponType != WeaponType.Ranged)
        {
            Debug.Log("<color=red>원거리(Ranged) 무기를 장착해야 비장의 한발을 사용할 수 있습니다!</color>");
            return false;
        }

        var stats = owner.GetComponentInChildren<PlayerStats>();
        var runner = owner.GetComponent<CoroutineRunner>();

        if (stats == null || runner == null) return false;

        // [조건 2] 마나 소모 (30)
        if (!stats.SpendMP(skillData.skillManaCost))
        {
            Debug.Log("<color=red>마나가 부족합니다!</color>");
            return false;
        }

        lastUsedTime = Time.time;
        runner.StartCoroutine(ExecutePiercingShot(owner, stats, weaponManager));
        return true;
    }

    private IEnumerator ExecutePiercingShot(GameObject owner, PlayerStats stats, WeaponManager weaponManager)
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

        // [조건 3] 공격력의 120% 데미지
        float baseAttack = stats.Attack.Value;
        this.finalSkillDamage = baseAttack * damageRatio;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        this.fireDirection = (mousePos - owner.transform.position).normalized;

        // 캐릭터 시선 방향 전환
        if (visualHandler != null && visualHandler.bodyAnimator != null)
        {
            visualHandler.bodyAnimator.SetFloat("MoveX", this.fireDirection.x);
            visualHandler.bodyAnimator.SetFloat("MoveY", this.fireDirection.y);
            
            Vector3 scale = visualHandler.bodyAnimator.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (this.fireDirection.x < 0 ? -1f : 1f);
            visualHandler.bodyAnimator.transform.localScale = scale;

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
        if (activeWeapon != null) activeWeapon.ExecuteAttack(this.fireDirection, 1.0f);

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
                projScript.Setup(projectileSpeed, projectileRange, finalSkillDamage, fireDirection, true);
            }
        }
    }
}