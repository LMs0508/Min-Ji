using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyData enemyData;

    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float detectionRange;
    [HideInInspector] public float stopDistance;
    [HideInInspector] public float knockbackForce;
    [HideInInspector] public float wanderSpeed;
    [HideInInspector] public float wanderDuration;
    [HideInInspector] public float waitDuration;
    [HideInInspector] public int maxHealth;

    void Awake()
    {
        ApplyData();
    }

    public void ApplyData()
    {
        if (enemyData == null)
        {
            return;
        }
        
         moveSpeed = enemyData.moveSpeed;
         detectionRange = enemyData.detectionRange;
         stopDistance = enemyData.stopDistance;
         knockbackForce = enemyData.knockbackForce;
         wanderSpeed = enemyData.wanderSpeed;
         wanderDuration = enemyData.wanderDuration;
         waitDuration = enemyData.waitDuration;
         maxHealth = enemyData.maxHealth;
    }
}
