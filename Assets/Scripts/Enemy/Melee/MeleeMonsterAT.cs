using UnityEngine;

public class MeleeMonsterAT : MonoBehaviour
{
    private EnemyStats stats;
    public MeleeArea meleeArea;

    void Start()
    {
        stats = GetComponentInParent<EnemyStats>();
    }

    public void OnMonsterHit()
    {
        if (meleeArea != null && stats != null && stats.enemyData != null)
        {
            float damage = stats.enemyData.damage;
            meleeArea.CheckAttack(damage);

            Debug.Log($"<color=cyan>[이벤트]</color> 공격 실행! 데미지: {damage}");
        }
    }
}