using UnityEngine;
using System.Collections.Generic; // 리스트(List) 기능을 쓰기 위해 필요해서 넣음.

public class EnemySpawner : MonoBehaviour
{
    [Header("데이터 및 프리팹")]
    public EnemyData enemyData; // spawnInterval 받아오기 위해서임
    public GameObject enemyPrefab; // 실제 생성할 몬스터의 프리팹

    [Header("스폰 제한")]
    public int maxEnemyCount = 5; // 이건 최대 스폰 숫자
    public float spawnRadius = 2f; // 스폰 범위

    protected float timer;
    protected List<GameObject> spawnedEnemies = new List<GameObject>(); // 스폰된 몬스터를 관리하기 위한 리스트

    protected virtual void Update()
    {
        CleanDeadEnemies(); // 죽은 몬스터는 리스트에서 삭제하는 함수

        if (enemyData == null)
            return;
        if(spawnedEnemies.Count < maxEnemyCount) //최대숫자보다 적을때 스폰 타이머가 작동함
        {
            timer += Time.deltaTime;

            if(timer >= enemyData.spawnInterval) // EnemyData에 설정된 시간으로 적용
            { // 설정한 시간이 되면 몬스터 스폰되도록 하는 거임
                SpawnEnemy();
                timer = 0f;
            }
        }
    }

    protected virtual void SpawnEnemy()
    {
        if (enemyPrefab == null)
            return;

        Vector2 spawnPos;
        int attempt = 0;
        float minSpawnDistance = 3f;

        do{
            spawnPos = GetRandomSpawnPosition();
            attempt++;
            if (GameObject.FindGameObjectWithTag("Player") == null) break;

            float distToPlayer = Vector2.Distance(spawnPos, GameObject.FindGameObjectWithTag("Player").transform.position);
            if (distToPlayer > minSpawnDistance) break;
        } while (attempt < 10);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        EnemyStats enemyStats = enemy.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            enemyStats.enemyData = this.enemyData; // 여기서 데이터를 전달함!
            enemyStats.ApplyData();
        }

        spawnedEnemies.Add(enemy);
    }
    // 스포너 주변의 무작위 좌표를 구하는 함수
    protected virtual Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius; // 원형 범위 안에서 무작위 좌표
        return (Vector2)transform.position + randomOffset;
    }

    // 리스트를 관리하기 위한 함수
    protected void CleanDeadEnemies()
    {
        // 이건 리스트를 뒤에서부터 검사해서 null이 있는지 있으면 그 항목을 지우는 역할을 함
        for(int i=spawnedEnemies.Count -1; i>=0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }
    // 스폰 범위때메 쓰는 함수
    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; // 하늘색으로 표시
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
