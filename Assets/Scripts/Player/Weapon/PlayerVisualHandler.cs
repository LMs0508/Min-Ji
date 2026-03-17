using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerVisualHandler : MonoBehaviour
{
    [Header("설정")]
    public float combatModeDuration = 5f;
    public bool isVisualLocked = false;
    
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
        if (bodyAnimator != null)
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
            bodyAnimator.SetFloat("MoveX", dir.x);
            bodyAnimator.SetFloat("MoveY", dir.y);

            // 좌우 이동 시 PlayerBody의 좌우 반전(Flip) 자동 처리
            // if (Mathf.Abs(dir.x) > 0.01f)
            // {
            //     Vector3 scale = bodyAnimator.transform.localScale;
            //     scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1f : 1f);
            //     bodyAnimator.transform.localScale = scale;
            // }
        }

        // =========================================================
        // [복구 완료] 이동 방향에 따라 등 뒤 무기의 앞뒤 레이어를 정렬합니다!
        // =========================================================
        UpdateWeaponSorting(dir);
    }

    private void UpdateWeaponSorting(Vector2 dir)
    {
        if (WeaponHolder == null || bodyAnimator == null) return;

        SpriteRenderer bodySR = bodyAnimator.GetComponent<SpriteRenderer>();
        if (bodySR != null)
        {
            bool isLookingUp = (Mathf.Abs(dir.y) > Mathf.Abs(dir.x)) && (dir.y > 0);
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
        SetSwordVisible(false); // 전투 시 등 뒤 무기 숨김

        yield return new WaitForSeconds(combatModeDuration);

        isCombatMode = false;
        SetSwordVisible(true);  // 일상 복귀 시 무기 표시
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
                // 무기 전용 애니메이션 세트로 덮어씌웁니다.
                bodyAnimator.runtimeAnimatorController = weapon.weaponAnimatorOverride;
                Debug.Log($"<color=lime>{weapon.itemName} 애니메이션 장착 완료!</color>");
            }
            else
            {
                // 장착 해제했거나 오버라이드가 없는 무기면 다시 순정(맨손) 상태로 복구합니다.
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
}