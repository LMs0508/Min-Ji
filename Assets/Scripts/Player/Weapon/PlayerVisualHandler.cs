using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerVisualHandler : MonoBehaviour
{
    [Header("모드 설정")]
    public float combatModeDuration = 5f;
    public bool isForcedCombatMode = false;
    public bool isVisualLocked = false;

    [Header("일반 모드 (Walk)")]
    public GameObject walkFront;
    public GameObject walkBack, walkRight, walkLeft;

    [Header("전투 모드 (WithWeapon)")]
    public GameObject withWeaponIdle;
    public GameObject dashRight, dashLeft;
    public Transform WeaponHolder;

    private Animator playerAnimator;
    private TopDownCharacterController controller;
    private SpriteRenderer bodyRenderer;
    private GameObject currentVisual;
    private bool isCombatMode = false;
    private Coroutine combatTimer;
    
    private BackWeaponVisual backWeaponVisual; 

    private void Awake()
    {
        controller = GetComponent<TopDownCharacterController>();
        bodyRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();

        // [수정] 자식 오브젝트에 실수로 넣었을 경우를 대비해 GetComponentInChildren 사용
        if (WeaponHolder != null)
        {
            backWeaponVisual = WeaponHolder.GetComponentInChildren<BackWeaponVisual>();
        }
    }

    private void Update()
    {
        if (isVisualLocked) return;
        UpdateAnimationState();
    }

    public void TriggerCombatMode()
    {
        if (combatTimer != null) StopCoroutine(combatTimer);
        combatTimer = StartCoroutine(CombatModeRoutine());
    }

    private IEnumerator CombatModeRoutine()
    {
        isCombatMode = true;
        SetSwordVisible(false); // 전투 모드 돌입 시 등 뒤 무기 숨김
        yield return new WaitForSeconds(combatModeDuration);

        isCombatMode = false;
        SetSwordVisible(true);  // 일상 복귀 시 등 뒤 무기 표시
        combatTimer = null;
    }

   public void ChangeBackWeapon(WeaponData weapon)
    {
        if (backWeaponVisual != null)
        {
            backWeaponVisual.ChangeWeapon(weapon);
            
            if (isCombatMode) SetSwordVisible(false);
        }

        if (playerAnimator != null && weapon.weaponAnimatorOverride != null)
        {
            playerAnimator.runtimeAnimatorController = weapon.weaponAnimatorOverride;
        }
    }

    private void SetSwordVisible(bool visible)
    {
        // 1. 전담 스크립트가 있다면 그것을 통해 제어
        if (backWeaponVisual != null)
        {
            backWeaponVisual.SetVisible(visible);
        }

        // =========================================================
        // 2. [핵심 복구] 스크립트 인식에 실패했거나, 자식에 스프라이트가 
        // 흩어져 있는 경우를 대비해 강력하게 강제로 다 꺼버립니다!
        // =========================================================
        if (WeaponHolder != null)
        {
            SpriteRenderer[] srs = WeaponHolder.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in srs)
            {
                // 건틀렛(투명도 0)인 상태에서 켜져도 알파값이 0이라 어차피 안 보이니 안전합니다.
                sr.enabled = visible;
            }
        }
    }

    private void UpdateAnimationState()
    {
        Vector2 dir = controller.MoveDirection;
        bool moving = controller.IsMoving;
        GameObject nextVisual = null;

        if (isCombatMode || isForcedCombatMode)
        {
            if (bodyRenderer) bodyRenderer.enabled = false;
            if (moving) nextVisual = (dir.x > 0) ? dashRight : dashLeft;
            else nextVisual = withWeaponIdle;
        }
        else
        {
            if (moving)
            {
                if (bodyRenderer) bodyRenderer.enabled = false;
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) nextVisual = (dir.x > 0) ? walkRight : walkLeft;
                else nextVisual = (dir.y > 0) ? walkBack : walkFront;
            }
            else
            {
                nextVisual = null;
                if (bodyRenderer) bodyRenderer.enabled = true;
            }
        }

        UpdateSwordSorting(nextVisual);

        if (currentVisual != nextVisual)
        {
            if (currentVisual) currentVisual.SetActive(false);
            if (nextVisual) nextVisual.SetActive(true);
            currentVisual = nextVisual;
        }
    }

    private void UpdateSwordSorting(GameObject nextVisual)
    {
        if (WeaponHolder == null || isCombatMode) return;

        int offset = (nextVisual == walkBack) ? 1 : -1;
        SpriteRenderer targetSR = (nextVisual != null) ? nextVisual.GetComponent<SpriteRenderer>() : bodyRenderer;

        if (targetSR != null)
        {
            // 자식에 있는 모든 스프라이트 렌더러의 Sorting을 잡아줍니다.
            foreach (var holderSR in WeaponHolder.GetComponentsInChildren<SpriteRenderer>())
            {
                holderSR.sortingLayerName = targetSR.sortingLayerName;
                holderSR.sortingOrder = targetSR.sortingOrder + offset;
            }
        }
    }
}