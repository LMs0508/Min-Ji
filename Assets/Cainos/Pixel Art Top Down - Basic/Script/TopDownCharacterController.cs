using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public float speed = 5f;
        public GameObject clickEffectPrefab; // 1. 방금 만든 프리팹을 넣을 칸

        private Animator animator;
        private Rigidbody2D rb;
        private Vector2 targetPosition;
        private bool isMoving = false;
        public float stoppingDistance = 0.1f;

        private void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            targetPosition = transform.position;
        }

        private void Update()
        {
            // 마우스 우클릭을 "눌렀을 때(Down)" 한 번만 이펙트 생성
            if (Input.GetMouseButtonDown(1))
            {
                ShowClickEffect();
            }

            // 우클릭을 "누르고 있는 동안"은 계속 목표지점 갱신
            if (Input.GetMouseButton(1))
            {
                SetTargetPosition();
            }

            MoveToTarget();
        }

        private void ShowClickEffect()
        {
            if (clickEffectPrefab == null) return;

            // 마우스 위치 계산 (좌표 변환)
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // 2. 이펙트 생성
            GameObject effect = Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);

            // 3. 0.5초 뒤에 자동으로 파괴 (애니메이션 길이에 맞춰 조절)
            Destroy(effect, 0.5f);
        }

        private void SetTargetPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -Camera.main.transform.position.z;
            targetPosition = Camera.main.ScreenToWorldPoint(mousePos);
            isMoving = true;
        }

        private void MoveToTarget()
        {
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (isMoving && distance > stoppingDistance)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                UpdateAnimator(direction);
                rb.linearVelocity = direction * speed;
                animator.SetBool("IsMoving", true);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("IsMoving", false);
                isMoving = false;
            }
        }

        private void UpdateAnimator(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                animator.SetInteger("Direction", dir.x > 0 ? 2 : 3);
            }
            else
            {
                animator.SetInteger("Direction", dir.y > 0 ? 1 : 0);
            }
        }
    }
}