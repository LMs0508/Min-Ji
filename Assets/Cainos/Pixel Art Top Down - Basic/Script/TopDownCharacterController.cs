using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems; // UI 체크를 위해 필요

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        [Header("이동 설정")]
        public float speed = 5f;
        public float stoppingDistance = 0.1f;
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

        // 내부 상태 변수
        private Rigidbody2D rb;
        private SpriteRenderer idleRenderer;
        private GameObject currentWalkObj;
        private Vector2 targetPosition;

        private bool isMoving = false;
        private bool isWeaponMode = false;
        private bool isDraggingFromUI = false; // UI에서 드래그 시작했는지 체크
        private Coroutine weaponModeCoroutine;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            idleRenderer = GetComponent<SpriteRenderer>();
            targetPosition = transform.position;

            AllVisualsOff();
        }

        private void Update()
        {
            HandleInput();
            MoveToTarget();
        }

        // --- 입력 처리 로직 (UI 방지 포함) ---
        private void HandleInput()
        {
            // 마우스 오른쪽 버튼(1) 클릭 시
            if (Input.GetMouseButtonDown(1))
            {
                // UI 위를 클릭했다면 이동 무시
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    isDraggingFromUI = true;
                    return;
                }

                isDraggingFromUI = false;
                ShowClickEffect();
            }

            // 클릭을 뗐을 때
            if (Input.GetMouseButtonUp(1))
            {
                isDraggingFromUI = false;
            }

            // 누르고 있는 동안 타겟 갱신 (UI에서 시작된 드래그가 아닐 때만)
            if (Input.GetMouseButton(1) && !isDraggingFromUI)
            {
                SetTargetPosition();
            }
        }

        // --- 무기 모드 제어 ---
        public void ActivateWeaponMode(float duration)
        {
            if (weaponModeCoroutine != null) StopCoroutine(weaponModeCoroutine);
            weaponModeCoroutine = StartCoroutine(WeaponModeTimer(duration));
        }

        private IEnumerator WeaponModeTimer(float duration)
        {
            isWeaponMode = true;

            // 등에 있는 무기 스프라이트 숨기기 (손에 들었으므로)
            if (skillHolder != null)
            {
                SpriteRenderer backWeaponSr = skillHolder.GetComponent<SpriteRenderer>();
                if (backWeaponSr != null) backWeaponSr.enabled = false;
            }

            if (!isMoving) StopAndShowIdle();

            yield return new WaitForSeconds(duration);

            isWeaponMode = false;

            // 등에 있는 무기 다시 보이기
            if (skillHolder != null)
            {
                SpriteRenderer backWeaponSr = skillHolder.GetComponent<SpriteRenderer>();
                if (backWeaponSr != null) backWeaponSr.enabled = true;
            }

            StopAndShowIdle();
            weaponModeCoroutine = null;
        }

        // --- 이동 및 비주얼 제어 ---
        private void MoveToTarget()
        {
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (isMoving && distance > stoppingDistance)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * speed;

                // 기본 Idle 스프라이트와 무기 Idle 오브젝트 숨기기
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

            // 걷기 애니메이션 오브젝트 모두 끄기
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

            SetWeaponSortingOrder(-1); // Idle 시 무기를 캐릭터 뒤로
        }

        private void UpdateWalkVisual(Vector2 dir)
        {
            GameObject nextWalkObj = null;

            if (isWeaponMode)
            {
                // 무기 모드일 때는 좌우 Dash만 사용
                nextWalkObj = (dir.x > 0) ? dashRight : dashLeft;
                SetWeaponSortingOrder(1); // 대시 중에는 무기를 캐릭터 앞으로
            }
            else
            {
                // 일반 모드일 때는 4방향 걷기 사용
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                {
                    nextWalkObj = (dir.x > 0) ? walkRight : walkLeft;
                    SetWeaponSortingOrder(-1);
                }
                else
                {
                    if (dir.y > 0) // 뒤로 걷기
                    {
                        nextWalkObj = walkBack;
                        SetWeaponSortingOrder(1); // 등에 멘 무기가 캐릭터 앞으로 와야 함
                    }
                    else // 앞으로 걷기
                    {
                        nextWalkObj = walkFront;
                        SetWeaponSortingOrder(-1);
                    }
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

        // 무기의 Sorting Order를 캐릭터 본체 기준 앞(+1) 또는 뒤(-1)로 조절
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