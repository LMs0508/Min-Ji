using System.Collections.Generic;
using System.Threading.Tasks;
using Cainos.PixelArtTopDown_Basic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class SceneFlowManager : MonoBehaviour
    {
        [Header("Floor scene names (must match scene file names)")]
        [SerializeField] private List<string> floorScenes = new List<string> { "floor01", "floor02","floor03" };

        [Header("Player")]
        [SerializeField] private Transform player;                       // PF Player �巡��
        [SerializeField] private string spawnObjectName = "FloorSpawn";  // �� Floor�� �ִ� ���� ������Ʈ �̸�

        [Header("Spawn")]
        [Tooltip("���� ��ġ�� ��Ȯ�� ��ġ�� Trigger Enter�� �� �ߴ� ���̽��� ���Ϸ��� ��¦ �������� �� �� �־��.")]
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [Tooltip("�� �ε�/��ε� �� �ߺ� ȣ�� ����")]
        [SerializeField] private bool blockWhileLoading = true;

        private int currentFloorIndex = -1;
        private Scene loadedFloorScene;
        private bool isLoading = false;

        private void Start()
        {
            // ���� ���� �� 1�� �ε�
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

                // 1) ���� Floor ��ε�
                if (currentFloorIndex >= 0)
                {
                    if (loadedFloorScene.IsValid() && loadedFloorScene.isLoaded)
                    {
                        var unloadOp = SceneManager.UnloadSceneAsync(loadedFloorScene);
                        while (unloadOp != null && !unloadOp.isDone)
                            await Task.Yield();
                    }
                }

                // 2) �� Floor Additive �ε�
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

                //  Active Scene ���� (Additive �������� ���� ���� ����)
                SceneManager.SetActiveScene(loadedFloorScene);

                currentFloorIndex = floorIndex;

                // 3) ���� ��ġ ã�� �÷��̾� �̵�
                MovePlayerToSpawn(loadedFloorScene);

                //  ����(Trigger/Collision) ������Ʈ �� ƽ �����ֱ�
                // (���� ���� Ʈ����/�浹�� ������ ���̽� ����)
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

            // transform.position을 먼저 설정해야 TopDownCharacterController.targetPosition이 올바른 위치를 읽음
            player.position = targetPos;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = targetPos;
                rb.WakeUp();
            }

            // targetPosition을 현재 위치로 초기화해서 자동 이동 방지
            var controller = player.GetComponent<Cainos.PixelArtTopDown_Basic.TopDownCharacterController>();
            controller?.StopMovement();
        }

        private GameObject FindSpawnObject(Scene floorScene)
        {
            // floor ���� ��Ʈ ������Ʈ���� "FloorSpawn" ã��
            foreach (var root in floorScene.GetRootGameObjects())
            {
                // 1) ��Ʈ ����
                var t = root.transform.Find(spawnObjectName);
                if (t != null) return t.gameObject;

                // 2) ��ü �˻�
                var all = root.GetComponentsInChildren<Transform>(true);
                foreach (var tr in all)
                {
                    if (tr.name == spawnObjectName)
                        return tr.gameObject;
                }
            }
            return null;
        }

        // (����) �ܺο��� ���� �� ��ȣ�� �ʿ��� ��
        public int CurrentFloorIndex => currentFloorIndex;
    }
}
