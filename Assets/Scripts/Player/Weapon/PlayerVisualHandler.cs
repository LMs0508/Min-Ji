using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerVisualHandler : MonoBehaviour
{
    [Header("모드 설정")]
    public float combatModeDuration = 5f;
    public bool isForcedCombatMode = false;

    // [핵심 추가] WeaponManager에서 공격 중일 때 시각 업데이트를 잠그기 위한 변수
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

    private void Awake()
    {
        controller = GetComponent<TopDownCharacterController>();
        bodyRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // [핵심 추가] 잠금 상태일 때는 아래의 애니메이션 업데이트 로직을 아예 실행하지 않습니다.
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
        SetSwordVisible(false); // 전투 모드 돌입 시 등 뒤의 검 숨김

        yield return new WaitForSeconds(combatModeDuration);

        isCombatMode = false;
        SetSwordVisible(true); // 일상 모드 복귀 시 등 뒤의 검 표시
        combatTimer = null;
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
                // [주의] 이 부분이 정자세 이미지를 강제로 켜는 부분인데, 
                // 이제 Update문 상단의 isVisualLocked 덕분에 공격 중에는 실행되지 않습니다.
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
            foreach (var sr in WeaponHolder.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.sortingLayerName = targetSR.sortingLayerName;
                sr.sortingOrder = targetSR.sortingOrder + offset;
            }
        }
    }

    private void SetSwordVisible(bool visible)
    {
        if (WeaponHolder == null) return;
        foreach (var sr in WeaponHolder.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = visible;
        }
    }
}