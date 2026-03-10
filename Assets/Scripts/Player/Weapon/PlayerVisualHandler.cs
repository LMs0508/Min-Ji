using UnityEngine;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;

public class PlayerVisualHandler : MonoBehaviour
{
    [Header("모드 설정")]
    public float combatModeDuration = 5f;
    public bool isForcedCombatMode = false;

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
        SetSwordVisible(false);

        yield return new WaitForSeconds(combatModeDuration);

        isCombatMode = false;
        SetSwordVisible(true);
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

        if (currentVisual != null)
        {
            Animator anim = currentVisual.GetComponent<Animator>();
            if (anim) anim.SetBool("IsMoving", moving);
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