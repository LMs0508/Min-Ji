using UnityEngine;
using System.Collections;

public class MiniSpiderSpawner : MonoBehaviour
{
    [SerializeField] private GameObject miniSpiderPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxCount = 5;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (CountAlive() < maxCount)
                SpawnOne();
        }
    }

    private void SpawnOne()
    {
        if (miniSpiderPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(miniSpiderPrefab, point.position, Quaternion.identity);
    }

    private int CountAlive()
    {
        int count = 0;
        foreach (var obj in FindObjectsByType<MiniSpiderController>(FindObjectsSortMode.None))
        {
            EnemyHealth hp = obj.GetComponent<EnemyHealth>();
            if (hp != null && !hp.IsDead) count++;
        }
        return count;
    }
}
