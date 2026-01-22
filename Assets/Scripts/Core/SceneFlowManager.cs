using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class SceneFlowManager : MonoBehaviour
    {
        [Header("Floor scene names (must match scene file names)")]
        [SerializeField] private List<string> floorScenes = new List<string> { "floor01", "floor02" };

        [Header("Player")]
        [SerializeField] private Transform player;                 // PF Player 드래그
        [SerializeField] private string spawnObjectName = "FloorSpawn"; // 각 Floor에 있는 스폰 오브젝트 이름

        private int currentFloorIndex = -1;
        private Scene loadedFloorScene;

        private void Start()
        {
            // 게임 시작 시 1층 로드
            LoadFloor(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
                LoadNextFloor();
        }

        public void LoadNextFloor()
        {
            LoadFloor(currentFloorIndex + 1);
        }

        public async void LoadFloor(int floorIndex)
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
                    await SceneManager.UnloadSceneAsync(loadedFloorScene);
                }
            }

            // 2) 새 Floor Additive 로드
            string sceneName = floorScenes[floorIndex];
            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!loadOp.isDone) await System.Threading.Tasks.Task.Yield();

            loadedFloorScene = SceneManager.GetSceneByName(sceneName);
            currentFloorIndex = floorIndex;

            // 3) 스폰 위치 찾고 플레이어 이동
            MovePlayerToSpawn(loadedFloorScene);
        }

        private void MovePlayerToSpawn(Scene floorScene)
        {
            if (player == null)
            {
                Debug.LogError("[SceneFlow] Player Transform is not assigned.");
                return;
            }

            // floor 씬의 루트 오브젝트에서 "FloorSpawn" 찾기
            GameObject spawn = null;
            foreach (var root in floorScene.GetRootGameObjects())
            {
                var t = root.transform.Find(spawnObjectName);
                if (t != null) { spawn = t.gameObject; break; }

                // 혹시 루트 직속이 아니면 전체 검색(조금 느리지만 안전)
                var all = root.GetComponentsInChildren<Transform>(true);
                foreach (var tr in all)
                {
                    if (tr.name == spawnObjectName)
                    {
                        spawn = tr.gameObject;
                        break;
                    }
                }
                if (spawn != null) break;
            }

            if (spawn == null)
            {
                Debug.LogError($"[SceneFlow] Spawn object '{spawnObjectName}' not found in {floorScene.name}");
                return;
            }

            player.position = spawn.transform.position;
        }
    }
}
