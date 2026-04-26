using System.Collections;
using Cainos.PixelArtTopDown_Basic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [SerializeField] private string targetScene;

    [Tooltip("목적 씬에서 어느 SpawnPoint로 이동할지 ID (PortalSpawnPoint의 SpawnID와 일치해야 함)")]
    [SerializeField] private string spawnID = "default";

    [Header("Key Lock (선택)")]
    [SerializeField] private ItemData requiredKey;
    [SerializeField] private string doorLabel = "문";
    [SerializeField, TextArea(1, 2)] private string[] noKeyLines = { "문이 미동도 없다." };

    private static float lastTransitionTime = -10f;
    private const float transitionCooldown = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastTransitionTime < transitionCooldown) return;

        if (requiredKey != null)
        {
            bool hasKey = InventoryManager.Instance != null
                          && InventoryManager.Instance.GetItemTotalCount(requiredKey) > 0;
            if (!hasKey)
            {
                if (DialogueManager.Instance != null)
                    DialogueManager.Instance.StartDialogue(null, doorLabel, noKeyLines, false);
                return;
            }
        }

        var controller = other.GetComponent<Cainos.PixelArtTopDown_Basic.TopDownCharacterController>();
        if (controller != null)
            controller.StopMovement();

        StartCoroutine(TransitionTo(targetScene, spawnID, controller));
    }

    private IEnumerator TransitionTo(string sceneName, string id, Cainos.PixelArtTopDown_Basic.TopDownCharacterController controller)
    {
        Scene fromScene = gameObject.scene;

        // 로딩 중 입력 차단
        if (controller != null) controller.enabled = false;

        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene toScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(toScene);

        DisableSceneCamera(toScene);
        MovePlayerToSpawn(toScene, id, controller);
        SnapCamera();

        // 컨트롤러 복구를 언로드 전에 처리 (언로드 시 이 오브젝트도 파괴되어 코루틴이 중단됨)
        if (controller != null)
            controller.enabled = true;

        lastTransitionTime = Time.time;

        yield return SceneManager.UnloadSceneAsync(fromScene);
    }

    private void MovePlayerToSpawn(Scene scene, string id, TopDownCharacterController controller)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var spawn in root.GetComponentsInChildren<PortalSpawnPoint>(true))
            {
                if (spawn.SpawnID != id) continue;

                // transform.position을 먼저 설정해야 StopMovement()가 올바른 위치를 읽음
                player.transform.position = spawn.transform.position;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.position = spawn.transform.position;
                }

                controller?.StopMovement();
                return;
            }
        }

        Debug.LogWarning($"[ScenePortal] SpawnID '{id}'를 {scene.name}에서 찾을 수 없습니다.");
    }

    private void SnapCamera()
    {
        // Camera.main 대신 씬 전체에서 CameraFollowS를 직접 검색
        var follow = Object.FindFirstObjectByType<CameraFollowS>();
        if (follow != null)
            follow.SnapToTarget();
    }

    private void DisableSceneCamera(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
            {
                cam.gameObject.SetActive(false);
            }
        }
    }
}
