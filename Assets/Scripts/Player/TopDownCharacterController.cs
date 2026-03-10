using UnityEngine;
using System.Collections;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public float speed = 5f;
        public GameObject clickEffectPrefab;

        [Header("Walking Objects (기본)")]
        public GameObject walkFront;
        public GameObject walkBack;
        public GameObject walkRight;
        public GameObject walkLeft;

        [Header("무기 상태 오브젝트 (WithWeapon)")]
        public GameObject withWeaponIdle;
        public GameObject dashRight;
        public GameObject dashLeft;

        [Header("무기 셋팅")]
        public Transform skillHolder; // 등 뒤의 무기 (SpriteRenderer가 붙어있는 곳)

        private SpriteRenderer idleRenderer;
        private GameObject currentWalkObj;
        private Rigidbody2D rb;
        private Vector2 targetPosition;
        private bool isMoving = false;
        public float stoppingDistance = 0.1f;

        private bool isWeaponMode = false;
        private Coroutine weaponModeCoroutine; // 코루틴 관리를 위한 변수

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            idleRenderer = GetComponent<SpriteRenderer>();
            targetPosition = transform.position;

            AllVisualsOff();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1)) ShowClickEffect();
            if (Input.GetMouseButton(1)) SetTargetPosition();

            MoveToTarget();
        }

        public void ActivateWeaponMode(float duration)
        {
            if (weaponModeCoroutine != null) StopCoroutine(weaponModeCoroutine);
            weaponModeCoroutine = StartCoroutine(WeaponModeTimer(duration));
        }

        private IEnumerator WeaponModeTimer(float duration)
        {
            isWeaponMode = true;

            SpriteRenderer backWeaponSr = skillHolder.GetComponent<SpriteRenderer>();
            if (backWeaponSr != null) backWeaponSr.enabled = false;

            if (!isMoving) StopAndShowIdle();

            yield return new WaitForSeconds(duration);

            isWeaponMode = false;
            if (backWeaponSr != null) backWeaponSr.enabled = true;

            StopAndShowIdle();
            weaponModeCoroutine = null;
        }

        private void MoveToTarget()
        {
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (isMoving && distance > stoppingDistance)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * speed;

                if (idleRenderer) idleRenderer.enabled = false;
                if (withWeaponIdle) withWeaponIdle.SetActive(false);
                UpdateWalkVisual(direction);
            }
            else
            {
                StopAndShowIdle();
            }
        }

        private void StopAndShowIdle()
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;

            if (currentWalkObj != null)
            {
                currentWalkObj.SetActive(false);
                currentWalkObj = null;
            }

            if (isWeaponMode)
            {
                if (idleRenderer) idleRenderer.enabled = false;
                if (withWeaponIdle) withWeaponIdle.SetActive(true);
            }
            else
            {
                if (withWeaponIdle) withWeaponIdle.SetActive(false);
                if (idleRenderer) idleRenderer.enabled = true;
            }

            SetWeaponSortingOrder(-1);
        }

        private void UpdateWalkVisual(Vector2 dir)
        {
            GameObject nextWalkObj = null;

            if (isWeaponMode)
            {
                nextWalkObj = (dir.x > 0) ? dashRight : dashLeft;
                SetWeaponSortingOrder(1);
            }
            else
            {
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                {
                    nextWalkObj = (dir.x > 0) ? walkRight : walkLeft;
                    SetWeaponSortingOrder(-1);
                }
                else
                {
                    if (dir.y > 0) { nextWalkObj = walkBack; SetWeaponSortingOrder(1); }
                    else { nextWalkObj = walkFront; SetWeaponSortingOrder(-1); }
                }
            }

            if (nextWalkObj != null && currentWalkObj != nextWalkObj)
            {
                if (currentWalkObj != null) currentWalkObj.SetActive(false);
                nextWalkObj.SetActive(true);
                currentWalkObj = nextWalkObj;
            }
        }

        private void AllVisualsOff()
        {
            if (walkFront) walkFront.SetActive(false);
            if (walkBack) walkBack.SetActive(false);
            if (walkRight) walkRight.SetActive(false);
            if (walkLeft) walkLeft.SetActive(false);
            if (withWeaponIdle) withWeaponIdle.SetActive(false);
            if (dashRight) dashRight.SetActive(false);
            if (dashLeft) dashLeft.SetActive(false);
        }

        private void SetWeaponSortingOrder(int offset)
        {
            if (skillHolder == null) return;

            SpriteRenderer baseSr = isWeaponMode ? withWeaponIdle.GetComponent<SpriteRenderer>() : idleRenderer;
            if (baseSr == null) return;

            SpriteRenderer[] weaponSrs = skillHolder.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in weaponSrs)
            {
                sr.sortingOrder = baseSr.sortingOrder + offset;
            }
        }

        private void SetTargetPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            targetPosition = Camera.main.ScreenToWorldPoint(mousePos);
            isMoving = true;
        }

        private void ShowClickEffect()
        {
            if (clickEffectPrefab == null) return;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            GameObject effect = Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }
}