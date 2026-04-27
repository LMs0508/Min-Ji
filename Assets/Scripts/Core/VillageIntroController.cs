using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using Cainos.PixelArtTopDown_Basic;

public class VillageIntroController : MonoBehaviour
{
    [Header("시네머신")]
    [Tooltip("Position Control: Do Nothing, Rotation Control: Do Nothing")]
    public CinemachineCamera villageCam;

    [Header("카메라 이동")]
    public Transform panStartPoint;
    public Transform panEndPoint;
    public float panDuration = 4f;
    public float holdAfterPan = 0.5f;

    [Header("페이드 타이밍")]
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    [Header("다음 씬")]
    public string nextScene = "InHouse";
    public string spawnID = "default";

    private void Start()
    {
        StartCoroutine(PlayVillageIntro());
    }

    private IEnumerator PlayVillageIntro()
    {
        // 플레이어 숨기기 (Village는 순수 시네마틱)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.SetActive(false);

        // VCam 활성화 → CameraFollowS 자동 정지
        villageCam.gameObject.SetActive(true);

        // 카메라 시작 위치 배치
        Vector3 startPos = new Vector3(panStartPoint.position.x, panStartPoint.position.y, villageCam.transform.position.z);
        Vector3 endPos = new Vector3(panEndPoint.position.x, panEndPoint.position.y, villageCam.transform.position.z);
        villageCam.transform.position = startPos;

        // 페이드 인 (GameScene에서 넘어온 후 밝아지기)
        if (FadeController.Instance != null)
            yield return StartCoroutine(FadeController.Instance.FadeIn(fadeInDuration));

        // 마을 전경 카메라 패닝
        float elapsed = 0f;
        while (elapsed < panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / panDuration);
            villageCam.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(holdAfterPan);

        // 페이드 아웃
        if (FadeController.Instance != null)
            yield return StartCoroutine(FadeController.Instance.FadeOut(fadeOutDuration));

        // InHouse 씬 전환 (ScenePortal과 동일한 Additive 방식)
        Scene fromScene = gameObject.scene;
        yield return SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);

        Scene toScene = SceneManager.GetSceneByName(nextScene);
        SceneManager.SetActiveScene(toScene);

        // 플레이어 복구 및 스폰 위치로 이동
        if (player != null)
        {
            player.SetActive(true);
            MovePlayerToSpawn(toScene, spawnID, player);
        }

        villageCam.gameObject.SetActive(false);

        yield return SceneManager.UnloadSceneAsync(fromScene);
    }

    private void MovePlayerToSpawn(Scene scene, string id, GameObject player)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var spawn in root.GetComponentsInChildren<PortalSpawnPoint>(true))
            {
                if (spawn.SpawnID != id) continue;
                player.transform.position = spawn.transform.position;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) { rb.linearVelocity = Vector2.zero; rb.position = spawn.transform.position; }
                return;
            }
        }
        Debug.LogWarning($"[VillageIntro] SpawnID '{id}'를 {scene.name}에서 찾을 수 없습니다.");
    }
}
