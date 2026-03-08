using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterControllers : MonoBehaviour
    {
        public float speed = 5f;
        public GameObject clickEffectPrefab;

        [Header("Visual Objects (자식 오브젝트 연결)")]
        public GameObject idleObj;
        public GameObject walkFront;
        public GameObject walkBack;
        public GameObject walkRight;
        public GameObject walkLeft;

        private GameObject currentActiveObj;
        private Rigidbody2D rb;
        private Vector2 targetPosition;
        private bool isMoving = false;
        public float stoppingDistance = 0.1f;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            targetPosition = transform.position;

            InitializeVisuals();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                ShowClickEffect();
            }

            if (Input.GetMouseButton(1))
            {
                SetTargetPosition();
            }

            MoveToTarget();
        }

        private void InitializeVisuals()
        {
            if (idleObj) idleObj.SetActive(false);
            if (walkFront) walkFront.SetActive(false);
            if (walkBack) walkBack.SetActive(false);
            if (walkRight) walkRight.SetActive(false);
            if (walkLeft) walkLeft.SetActive(false);

            SwitchVisual(idleObj);
        }

        private void MoveToTarget()
        {
            float distance = Vector2.Distance(transform.position, targetPosition);

            if (isMoving && distance > stoppingDistance)
            {
                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;

                UpdateVisualDirection(direction);

                rb.linearVelocity = direction * speed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                SwitchVisual(idleObj);
            }
        }

        private void UpdateVisualDirection(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                if (dir.x > 0) SwitchVisual(walkRight);
                else SwitchVisual(walkLeft);
            }
            else
            {
                if (dir.y > 0) SwitchVisual(walkBack);
                else SwitchVisual(walkFront);
            }
        }

        private void SwitchVisual(GameObject target)
        {
            if (target == null || currentActiveObj == target) return;

            if (currentActiveObj != null) currentActiveObj.SetActive(false);
            target.SetActive(true);
            currentActiveObj = target;
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