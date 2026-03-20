using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerVisualHandler : MonoBehaviour
{
    [Header("설정")]
    public float combatModeDuration = 5f;
    public bool isVisualLocked = false;
    public bool isAttacking = false;
    
    public Animator bodyAnimator; 
    public Transform WeaponHolder;

    private TopDownCharacterController controller;
    private bool isCombatMode = false;
    private Coroutine combatTimer;
    private BackWeaponVisual backWeaponVisual; 

    private RuntimeAnimatorController baseController;

    private void Awake()
    {
        controller = GetComponent<TopDownCharacterController>();
        if (WeaponHolder != null) backWeaponVisual = WeaponHolder.GetComponentInChildren<BackWeaponVisual>();
        
        if (bodyAnimator == null) bodyAnimator = GetComponentInChildren<Animator>();
        if (bodyAnimator != null && baseController == null)
        {
            baseController = bodyAnimator.runtimeAnimatorController;
        }
    }

    private void Update()
    {
        if (isVisualLocked || bodyAnimator == null) return;

        Vector2 dir = controller.MoveDirection;
        bool isMoving = controller.IsMoving;

        bodyAnimator.SetBool("IsMoving", isMoving);
        bodyAnimator.SetBool("IsCombat", isCombatMode);

        if (isMoving)
        {
            if (!isAttacking)
            {
                bodyAnimator.SetFloat("MoveX", dir.x);
                bodyAnimator.SetFloat("MoveY", dir.y);

                // =========================================================
                // 1. [평소 이동 시] 키보드 방향에 따라 몸통 뒤집기!
                // =========================================================
                if (Mathf.Abs(dir.x) > 0.01f)
                {
                    Vector3 scale = bodyAnimator.transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1f : 1f);
                    bodyAnimator.transform.localScale = scale;
                }
            }
        }

        UpdateWeaponSorting(dir, isMoving);
    }

    private void UpdateWeaponSorting(Vector2 dir, bool isMoving)
    {
        if (WeaponHolder == null || bodyAnimator == null) return;

        SpriteRenderer bodySR = bodyAnimator.GetComponent<SpriteRenderer>();
        if (bodySR != null)
        {
            bool isLookingUp = isMoving && (Mathf.Abs(dir.y) > Mathf.Abs(dir.x)) && (dir.y > 0);
            int offset = isLookingUp ? 1 : -1;

            foreach (var sr in WeaponHolder.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.sortingLayerName = bodySR.sortingLayerName;
                sr.sortingOrder = bodySR.sortingOrder + offset;
            }
        }
    }

    public void TriggerCombatMode()
    {
        if (combatTimer != null) StopCoroutine(combatTimer);
        combatTimer = StartCoroutine(CombatModeRoutine());
    }

    private IEnumerator CombatModeRoutine()
    {
        isCombatMode = true;
        SetSwordVisible(false);

        yield return new WaitForSeconds(combatModeDuration);

        isCombatMode = false;
        SetSwordVisible(true);
        combatTimer = null;
    }

    public void ChangeBackWeapon(WeaponData weapon)
    {
        if (backWeaponVisual != null)
        {
            backWeaponVisual.ChangeWeapon(weapon);
            if (isCombatMode) SetSwordVisible(false);
        }
        if (bodyAnimator != null)
        {
            if (weapon != null && weapon.weaponAnimatorOverride != null)
            {
                bodyAnimator.runtimeAnimatorController = weapon.weaponAnimatorOverride;
                Debug.Log($"<color=lime>{weapon.itemName} 애니메이션 장착 완료!</color>");
            }
            else
            {
                bodyAnimator.runtimeAnimatorController = baseController;
            }
        }
    }

    private void SetSwordVisible(bool visible)
    {
        if (backWeaponVisual != null) backWeaponVisual.SetVisible(visible);
        if (WeaponHolder != null)
        {
            foreach (var sr in WeaponHolder.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = visible;
        }
    }

    public void PlayAttackAnimation(Vector2 attackDir, int comboStep = 0)
    {
        if (bodyAnimator == null) return;

        // =========================================================
        // 2. [공격 시] 마우스 방향에 맞춰 몸통 뒤집기!
        // =========================================================
        if (Mathf.Abs(attackDir.x) > 0.01f)
        {
            Vector3 scale = bodyAnimator.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (attackDir.x < 0 ? -1f : 1f);
            bodyAnimator.transform.localScale = scale;
        }

        // 혹시 남아있을지 모르는 FlipX 귀신 박멸
        SpriteRenderer sr = bodyAnimator.GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = false; 

        // 콤보 단계(1타=0, 2타=1, 3타=2)를 애니메이터에 전달
        bodyAnimator.SetInteger("ComboStep", comboStep);

        bodyAnimator.SetFloat("MoveX", attackDir.x);
        bodyAnimator.SetFloat("MoveY", attackDir.y);
        
        bodyAnimator.SetTrigger("Attack");
    }
}