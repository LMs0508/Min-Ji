using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class SceneFlowManager : MonoBehaviour
    {
        [Header("Floor scene names (must match scene file names)")]
        [SerializeField] private List<string> floorScenes = new List<string> { "floor01", "floor02" };

        [Header("Player")]
        [SerializeField] private Transform player;                       // PF Player 드래그
        [SerializeField] private string spawnObjectName = "FloorSpawn";  // 각 Floor에 있는 스폰 오브젝트 이름

        [Header("Spawn")]
        [Tooltip("스폰 위치에 정확히 겹치면 Trigger Enter가 안 뜨는 케이스를 피하려고 살짝 오프셋을 줄 수 있어요.")]
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [Tooltip("층 로드/언로드 중 중복 호출 방지")]
        [SerializeField] private bool blockWhileLoading = true;

        private int currentFloorIndex = -1;
        private Scene loadedFloorScene;
        private bool isLoading = false;

        private void Start()
        {
            // 게임 시작 시 1층 로드
            _ = LoadFloorAsync(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
                _ = LoadNextFloorAsync();
        }

        public Task LoadNextFloorAsync()
        {
            return LoadFloorAsync(currentFloorIndex + 1);
        }

        public async Task LoadFloorAsync(int floorIndex)
        {
            if (blockWhileLoading && isLoading) return;
            isLoading = true;

            try
            {
                if (floorIndex < 0 || floorIndex >= floorScenes.Count)
                {
                    Debug.LogWarning($"[SceneFlow] Invalid floor index: {floorIndex}");
                    return;
                }

                // 1) 기존 Floor 언로드
                if (currentFloorIndex >= 0)
                {
                    if (loadedFloorScene.IsValid() && loadedFloorScene.isLoaded)
                    {
                        var unloadOp = SceneManager.UnloadSceneAsync(loadedFloorScene);
                        while (unloadOp != null && !unloadOp.isDone)
                            await Task.Yield();
                    }
                }

                // 2) 새 Floor Additive 로드
                string sceneName = floorScenes[floorIndex];
                var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (loadOp != null && !loadOp.isDone)
                    await Task.Yield();

                loadedFloorScene = SceneManager.GetSceneByName(sceneName);
                if (!loadedFloorScene.IsValid() || !loadedFloorScene.isLoaded)
                {
                    Debug.LogError($"[SceneFlow] Failed to load scene: {sceneName}");
                    return;
                }

                //  Active Scene 설정 (Additive 구성에서 각종 꼬임 방지)
                SceneManager.SetActiveScene(loadedFloorScene);

                currentFloorIndex = floorIndex;

                // 3) 스폰 위치 찾고 플레이어 이동
                MovePlayerToSpawn(loadedFloorScene);

                //  물리(Trigger/Collision) 업데이트 한 틱 돌려주기
                // (스폰 직후 트리거/충돌이 씹히는 케이스 방지)
                await Task.Yield();
                Physics2D.SyncTransforms();
            }
            finally
            {
                isLoading = false;
            }
        }

        private void MovePlayerToSpawn(Scene floorScene)
        {
            if (player == null)
            {
                Debug.LogError("[SceneFlow] Player Transform is not assigned.");
                return;
            }

            var spawn = FindSpawnObject(floorScene);
            if (spawn == null)
            {
                Debug.LogError($"[SceneFlow] Spawn object '{spawnObjectName}' not found in {floorScene.name}");
                return;
            }

            Vector2 targetPos = (Vector2)spawn.transform.position + spawnOffset;

            //  Rigidbody2D가 있으면 물리 위치로 순간이동 (Trigger/Collision 안정)
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = targetPos;      // 핵심: transform.position 대신 rb.position
                rb.WakeUp();
            }
            else
            {
                player.position = targetPos;
            }
        }

        private GameObject FindSpawnObject(Scene floorScene)
        {
            // floor 씬의 루트 오브젝트에서 "FloorSpawn" 찾기
            foreach (var root in floorScene.GetRootGameObjects())
            {
                // 1) 루트 직속
                var t = root.transform.Find(spawnObjectName);
                if (t != null) return t.gameObject;

                // 2) 전체 검색
                var all = root.GetComponentsInChildren<Transform>(true);
                foreach (var tr in all)
                {
                    if (tr.name == spawnObjectName)
                        return tr.gameObject;
                }
            }
            return null;
        }

        // (선택) 외부에서 현재 층 번호가 필요할 때
        public int CurrentFloorIndex => currentFloorIndex;
    }
}
