using UnityEngine;
using System.Collections;

public class MagicGauntlet : WeaponBase
{
    private int comboStep = 0;         
    private float lastAttackTime = 0f;  
    public float comboLimit = 2.0f;     // 콤보 허용 시간 (2초)
    public float attackDuration = 0.4f; // 일반 공격 딜레이
    
    private bool isAttacking = false;

    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        if (isAttacking) return;

        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        if (Time.time - lastAttackTime > comboLimit)
        {
            comboStep = 0;
        }

        lastAttackTime = Time.time;

        int currentCombo = comboStep; 

        PlayerVisualHandler visualHandler = wm.GetComponent<PlayerVisualHandler>();
        if (visualHandler != null)
        {
            visualHandler.isAttacking = true;
            visualHandler.PlayAttackAnimation(direction, currentCombo); // 현재 콤보 스텝 전달!
        }

        float currentAtk = wm.GetCurrentPlayerAttack();
        float currentMag = wm.GetCurrentPlayerMagic();

        float atkRatio = 0.5f;
        float magRatio = 0.5f;

        if (currentCombo == 2) // 마지막 3타 (80% / 80%)
        {
            atkRatio = 0.8f;
            magRatio = 0.8f;
        }

        int finalDamage = Mathf.RoundToInt((currentAtk * atkRatio + currentMag * magRatio) * multiplier);

        float range = (currentCombo == 2) ? data.attackRange * 1.5f : data.attackRange;
        Vector2 attackPoint = (Vector2)transform.position + (direction * 0.5f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, range);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
            }
        }

        if (currentCombo == 2)
            Debug.Log($"<color=orange><b>[3타 강공격!]</b></color> 데미지: {finalDamage}");
        else
            Debug.Log($"{currentCombo + 1}타 적중! 데미지: {finalDamage}");

        comboStep++;

        if (comboStep > 2)
        {
            comboStep = 0;
        }
        StartCoroutine(AttackRoutine(visualHandler, currentCombo));
    }

    private IEnumerator AttackRoutine(PlayerVisualHandler visualHandler, int currentCombo)
    {
        isAttacking = true;
        
        // 3타(마지막 타)일 때는 딜레이를 조금 더 길게 주어 타격감을 높입니다.
        float duration = (currentCombo == 2) ? attackDuration * 1.5f : attackDuration; 
        yield return new WaitForSeconds(duration);

        if (visualHandler != null)
        {
            visualHandler.isAttacking = false;
        }
        isAttacking = false;
    }
}