using UnityEngine;
using UnityEngine.EventSystems;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        [Header("이동 설정")]
        public float speed = 5f;
        public float stoppingDistance = 0.1f;
        public GameObject clickEffectPrefab;

        public bool IsMoving { get; private set; }
        public Vector2 MoveDirection { get; private set; }

        private Rigidbody2D rb;
        private Vector2 targetPosition;
        private bool isDraggingFromUI = false;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            targetPosition = transform.position;
            MoveDirection = Vector2.down; // 기본 정면 응시
        }

        private void Update()
        {
            HandleInput();
            MoveToTarget();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(1)) // 우클릭
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    isDraggingFromUI = true;
                    return;
                }
                isDraggingFromUI = false;
                ShowClickEffect();
            }

            if (Input.GetMouseButtonUp(1)) isDraggingFromUI = false;

            if (Input.GetMouseButton(1) && !isDraggingFromUI)
            {
                targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            }
        }

        private void MoveToTarget()
        {
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (distance > stoppingDistance)
            {
                IsMoving = true;
                MoveDirection = (targetPosition - (Vector2)transform.position).normalized;
                rb.linearVelocity = MoveDirection * speed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                IsMoving = false;
            }
        }

        private void ShowClickEffect()
        {
            if (clickEffectPrefab == null) return;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            Destroy(Instantiate(clickEffectPrefab, worldPos, Quaternion.identity), 0.5f);
        }

        public void StopMovement()
        {
            targetPosition = transform.position;
            IsMoving = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
}