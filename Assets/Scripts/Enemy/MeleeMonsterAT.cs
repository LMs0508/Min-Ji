using UnityEngine;

public class MeleeMonsterAT : EnemyBase
{
    public MeleeArea meleeArea;

    protected override void PerformAttack()
    {
        if (meleeArea != null)
        {
            meleeArea.CheckAttack((int)attackDamage);
        }
    }
    
}