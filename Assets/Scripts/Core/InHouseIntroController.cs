using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using Cainos.PixelArtTopDown_Basic;

public class InHouseIntroController : MonoBehaviour
{
    // 게임 처음 시작할 때만 실행 (InHouse를 나중에 다시 방문해도 스킵)
    private static bool hasPlayed = false;

    [Header("시네머신")]
    [Tooltip("Tracking Target: 플레이어, Position Control: Do Nothing (줌만 사용)")]
    public CinemachineCamera introCam;

    [Header("줌 설정")]
    public float closeupOrthoSize = 3f;
    public float normalOrthoSize = 5f;
    public float holdDuration = 1.5f;
    public float zoomOutDuration = 1.5f;

    [Header("페이드")]
    public float fadeInDuration = 1f;

    private void Start()
    {
        if (hasPlayed) { enabled = false; return; }
        StartCoroutine(PlayInHouseIntro());
    }

    private IEnumerator PlayInHouseIntro()
    {
        hasPlayed = true;

        // 플레이어 이동 정지
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Rigidbody2D playerRb = player?.GetComponent<Rigidbody2D>();
        TopDownCharacterController controller = player?.GetComponent<TopDownCharacterController>();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        if (controller != null) controller.enabled = false;

        // 카메라를 플레이어 위치에 스냅
        CameraFollowS cameraFollow = Camera.main?.GetComponent<CameraFollowS>();
        if (cameraFollow != null) cameraFollow.SnapToTarget();

        // 인트로 VCam 클로즈업 상태로 활성화
        var lens = introCam.Lens;
        lens.OrthographicSize = closeupOrthoSize;
        introCam.Lens = lens;
        introCam.gameObject.SetActive(true);

        // 페이드 인 (검은 화면 → 주인공 등장)
        if (FadeController.Instance != null)
            yield return StartCoroutine(FadeController.Instance.FadeIn(fadeInDuration));

        // 클로즈업 홀드
        yield return new WaitForSeconds(holdDuration);

        // 줌 아웃 (클로즈업 → 일반 시점)
        float elapsed = 0f;
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / zoomOutDuration);
            var l = introCam.Lens;
            l.OrthographicSize = Mathf.Lerp(closeupOrthoSize, normalOrthoSize, t);
            introCam.Lens = l;
            yield return null;
        }

        // VCam 끄기 → CameraFollowS가 자동으로 플레이어 추적 재개
        introCam.gameObject.SetActive(false);

        // 플레이어 이동 재개
        if (playerRb != null) playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (controller != null) controller.enabled = true;
    }
}
