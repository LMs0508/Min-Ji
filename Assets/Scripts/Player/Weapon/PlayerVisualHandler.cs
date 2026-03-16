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

    private TopDownCharacterController controller;
    private SpriteRenderer bodyRenderer;
    private GameObject currentVisual;
    private bool isCombatMode = false;
    private Coroutine combatTimer;
    
    // [신규 추가] WeaponHolder를 관리할 전담 스크립트 연결용
    private BackWeaponVisual backWeaponVisual; 

    private void Awake()
    {
        controller = GetComponent<TopDownCharacterController>();
        bodyRenderer = GetComponent<SpriteRenderer>();

        // 자식에 있는 BackWeaponVisual을 자동으로 찾습니다.
        if (WeaponHolder != null)
        {
            backWeaponVisual = WeaponHolder.GetComponent<BackWeaponVisual>();
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

    // 외부(WeaponManager)에서 무기를 바꿨을 때 호출되는 함수
    public void ChangeBackWeapon(WeaponData weapon)
    {
        // 이제 복잡한 로직 없이 전담 스크립트에게 쿨하게 넘깁니다.
        if (backWeaponVisual != null)
        {
            backWeaponVisual.ChangeWeapon(weapon);
        }
    }

    private void SetSwordVisible(bool visible)
    {
        // 전담 스크립트에게 켜고 끄라고 지시
        if (backWeaponVisual != null)
        {
            backWeaponVisual.SetVisible(visible);
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
            SpriteRenderer holderSR = WeaponHolder.GetComponent<SpriteRenderer>();
            if (holderSR != null)
            {
                holderSR.sortingLayerName = targetSR.sortingLayerName;
                holderSR.sortingOrder = targetSR.sortingOrder + offset;
            }
        }
    }
}