using UnityEngine;

public class SwordWeapon : WeaponBase
{
    // Gizmos를 그리기 위해 공격 지점을 변수로 저장
    private Vector2 lastAttackPoint;
    private float lastAttackRange;

    public override void ExecuteAttack(Vector2 direction)
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        float playerAtk = (wm != null) ? wm.GetCurrentPlayerAttack() : 0;
        float finalDamage = playerAtk * 1.0f;

        // 1. 공격 중심점 계산 (플레이어 위치에서 바라보는 방향으로 약간 앞)
        // 0.5f는 플레이어 몸 안에서 공격이 시작되지 않도록 하는 오프셋입니다.
        Vector2 attackPoint = (Vector2)transform.position + (direction * 0.5f);

        // 2. WeaponData에 설정된 사거리(attackRange) 가져오기
        float range = data.attackRange;

        // 디버그용 데이터 저장
        lastAttackPoint = attackPoint;
        lastAttackRange = range;

        // 3. 원형 범위 내의 모든 콜라이더 검출
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, range);

        foreach (Collider2D enemy in hitEnemies)
        {
            // 태그 확인 및 데미지 전달
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log($"{enemy.name}에게 {data.attackDamage}의 근접 데미지!");
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
            }
        }
    }

    // [중요] 유니티 에디터에서 공격 범위가 보이게 그려주는 기능
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastAttackPoint, lastAttackRange);
    }
}