using UnityEngine;

public class MeleeMonsterAT : MonoBehaviour
{
    private EnemyAI ai; // 추격 관련
    private EnemyHealth health; // 체력관련
    public MeleeArea meleeArea; // 공격 판정 관련

    void Awake()
    {
        ai = GetComponent<EnemyAI>();
        health = GetComponent<EnemyHealth>();
    }

    // 공격 애니메이션 이벤트 등을 위한 코드
    public void OnAttackEvent()
    {
        if (meleeArea != null)
        {
            // EnemyStats에서 공격력 가져오도록 연결함
            int damage = (int)GetComponent<EnemyStats>().enemyData.attackDamage;
            meleeArea.CheckAttack(damage);
        }
    }
}