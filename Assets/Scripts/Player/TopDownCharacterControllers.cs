using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterControllers : MonoBehaviour
    {
        public float speed = 5f;
        public GameObject clickEffectPrefab;

        [Header("Walking Objects (자식 오브젝트들)")]
        public GameObject walkFront;
        public GameObject walkBack;
        public GameObject walkRight;
        public GameObject walkLeft;

        private SpriteRenderer idleRenderer; 
        private GameObject currentWalkObj;   
        private Rigidbody2D rb;
        private Vector2 targetPosition;
        private bool isMoving = false;
        public float stoppingDistance = 0.1f;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            // 부모(나 자신)에게 붙어있는 Idle용 SpriteRenderer를 가져옵니다.
            idleRenderer = GetComponent<SpriteRenderer>();

            targetPosition = transform.position;

            // 초기화: 모든 자식 걷기 오브젝트를 끕니다.
            if (walkFront) walkFront.SetActive(false);
            if (walkBack) walkBack.SetActive(false);
            if (walkRight) walkRight.SetActive(false);
            if (walkLeft) walkLeft.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1)) ShowClickEffect();
            if (Input.GetMouseButton(1)) SetTargetPosition();

            MoveToTarget();
        }

        private void MoveToTarget()
        {
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (isMoving && distance > stoppingDistance)
            {
                // [1. 이동 중 로직]
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * speed;

                // 부모의 Idle 이미지는 숨기고, 방향에 맞는 걷기 오브젝트를 켭니다.
                if (idleRenderer) idleRenderer.enabled = false;
                UpdateWalkVisual(direction);
            }
            else
            {
                // [2. 정지 로직]
                StopAndShowIdle();
            }
        }

        private void StopAndShowIdle()
        {
            rb.linearVelocity = Vector2.zero;
            isMoving = false;

            // 걷기 오브젝트를 모두 끄고, 부모의 Idle 이미지를 다시 보여줍니다.
            if (currentWalkObj != null)
            {
                currentWalkObj.SetActive(false);
                currentWalkObj = null;
            }

            if (idleRenderer) idleRenderer.enabled = true;
        }

        private void UpdateWalkVisual(Vector2 dir)
        {
            GameObject nextWalkObj = null;

            // 상하좌우 판단
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                nextWalkObj = (dir.x > 0) ? walkRight : walkLeft;
            }
            else
            {
                nextWalkObj = (dir.y > 0) ? walkBack : walkFront;
            }

            // 오브젝트 교체 실행
            if (nextWalkObj != null && currentWalkObj != nextWalkObj)
            {
                if (currentWalkObj != null) currentWalkObj.SetActive(false);
                nextWalkObj.SetActive(true);
                currentWalkObj = nextWalkObj;
            }
        }

        // --- 마우스 클릭 및 이펙트 로직 (기존과 동일) ---
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