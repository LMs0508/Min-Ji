using UnityEngine;

public class MonsterAnimationBridge : MonoBehaviour
{
    private MeleeArea meleeArea;

    void Start()
    {
        meleeArea = GetComponentInChildren<MeleeArea>();
    }

    public void OnMonsterHit()
    {
        if (meleeArea != null)
        {
            var stats = GetComponentInParent<EnemyStats>();
            if (stats != null && stats.enemyData != null)
            {
                float realDamage = stats.enemyData.damage;
                meleeArea.CheckAttack(realDamage);
                Debug.Log($"<color=cyan>[브릿지]</color> 데이터에서 가져온 데미지: {realDamage}");
            }
            else
            {
                Debug.LogWarning("EnemyStats를 찾을 수 없습니다!");
            }
        }
    }
}