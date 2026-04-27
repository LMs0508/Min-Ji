using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class BossIntroController : MonoBehaviour
    {
        [Header("카메라 이동")]
        public Transform panStartPoint;
        public Transform panEndPoint;

        [Header("연출 타이밍")]
        public float panDuration = 2.5f;
        public float holdDuration = 0.5f;

        [Header("카메라 흔들림")]
        public float shakeMagnitude = 0.2f;
        public float shakeFrequency = 20f;
        public float shakeDuration = 0.5f;

        [Header("보스 대사")]
        public string bossName = "거미 보스";
        [TextArea] public string[] bossIntroLines;

        [Header("참조")]
        public SpiderBossController bossController;

        private void Start()
        {
            StartCoroutine(PlayIntro());
        }

        private IEnumerator PlayIntro()
        {
            Camera mainCam = Camera.main;
            CameraFollowS cameraFollow = mainCam != null ? mainCam.GetComponent<CameraFollowS>() : null;

            // 플레이어 이동 정지
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Rigidbody2D playerRb = player != null ? player.GetComponent<Rigidbody2D>() : null;
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            // 보스 비활성화
            if (bossController != null) bossController.enabled = false;

            // 한 프레임 대기 (씬 전환 후 UI가 모두 활성화될 때까지)
            yield return null;

            // DialoguePanel을 제외한 Canvas 자식 UI들 숨기기
            List<GameObject> hiddenUIObjects = HideUIExceptDialogue();

            // CameraFollowS 끄기
            if (cameraFollow != null) cameraFollow.enabled = false;

            // 카메라 시작 위치(하단) 배치
            Vector3 startPos = new Vector3(panStartPoint.position.x, panStartPoint.position.y, mainCam.transform.position.z);
            Vector3 endPos = new Vector3(panEndPoint.position.x, panEndPoint.position.y, mainCam.transform.position.z);
            mainCam.transform.position = startPos;

            // 하단 → 상단으로 이동
            float elapsed = 0f;
            while (elapsed < panDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / panDuration);
                mainCam.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            // 보스 위치에서 잠시 고정
            yield return new WaitForSeconds(holdDuration);

            // 보스 대사
            if (bossIntroLines != null && bossIntroLines.Length > 0 && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(null, bossName, bossIntroLines);
                yield return new WaitUntil(() => !DialogueManager.Instance.IsOpen());
            }

            // 카메라 흔들기
            yield return StartCoroutine(ShakeCamera(mainCam));

            // CameraFollowS 다시 켜기 → 카메라가 플레이어에게 복귀
            if (cameraFollow != null)
            {
                cameraFollow.SnapToTarget();
                cameraFollow.enabled = true;
            }

            // UI 다시 보이기
            foreach (var go in hiddenUIObjects)
                if (go != null) go.SetActive(true);

            // 플레이어 이동 재개
            if (playerRb != null)
                playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 보스 전투 시작
            if (bossController != null)
            {
                bossController.enabled = true;
                bossController.currentState = SpiderBossState.Chase;
            }
        }

        private List<GameObject> HideUIExceptDialogue()
        {
            if (DialogueManager.Instance == null || DialogueManager.Instance.panel == null)
                return new List<GameObject>();

            // 루트 Canvas 찾기
            Canvas rootCanvas = DialogueManager.Instance.panel.GetComponentInParent<Canvas>();
            if (rootCanvas == null) return new List<GameObject>();

            // DialoguePanel이 속한 Canvas의 직접 자식 찾기
            Transform dialogueChild = DialogueManager.Instance.panel.transform;
            while (dialogueChild.parent != rootCanvas.transform)
                dialogueChild = dialogueChild.parent;

            // 루트 Canvas의 직접 자식들 중 DialoguePanel 제외하고 숨기기
            var hidden = new List<GameObject>();
            foreach (Transform child in rootCanvas.transform)
            {
                if (child == dialogueChild) continue;
                if (!child.gameObject.activeSelf) continue;

                child.gameObject.SetActive(false);
                hidden.Add(child.gameObject);
            }
            return hidden;
        }

        private IEnumerator ShakeCamera(Camera cam)
        {
            Vector3 originalPos = cam.transform.position;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = originalPos.x + Mathf.Sin(elapsed * shakeFrequency) * shakeMagnitude;
                float y = originalPos.y + Mathf.Sin(elapsed * shakeFrequency * 1.3f) * shakeMagnitude;
                cam.transform.position = new Vector3(x, y, originalPos.z);
                yield return null;
            }

            cam.transform.position = originalPos;
        }
    }
}
