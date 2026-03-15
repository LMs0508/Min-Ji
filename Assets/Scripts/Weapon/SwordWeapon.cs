using System.Collections;
using UnityEngine;

public class SwordWeapon : WeaponBase
{
    [Header("▼ 비주얼 & 애니메이션")]
    public GameObject attackVisualObject;

    [Tooltip("공격 애니메이션이 재생되는 시간(초)을 입력하세요.")]
    public float attackDuration = 0.5f;

    private Vector2 lastAttackPoint;
    private float lastAttackRange;

    // 공격 중인지 체크해서 연타 방지
    private bool isAttacking = false;

    public override void ExecuteAttack(Vector2 direction)
    {
        // 이미 공격 중이면 중복 실행 방지
        if (isAttacking) return;

        WeaponManager wm = GetComponentInParent<WeaponManager>();
        float playerAtk = (wm != null) ? wm.GetCurrentPlayerAttack() : 0;
        float finalDamage = playerAtk * 1.0f;

        // 1. 데미지 판정 범위 계산
        Vector2 attackPoint = (Vector2)transform.position + (direction * 0.5f);
        float range = data.attackRange;

        lastAttackPoint = attackPoint;
        lastAttackRange = range;

        // 2. 데미지 적용
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, range);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log($"{enemy.name}에게 {finalDamage}의 근접 데미지!");
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
            }
        }

        // 3. [핵심] 공격 애니메이션 및 비주얼 교체 코루틴 실행
        if (wm != null)
        {
            StartCoroutine(AttackRoutine(wm, direction));
        }
    }

    private IEnumerator AttackRoutine(WeaponManager wm, Vector2 direction)
    {
        isAttacking = true;

        // 1. [개선] WeaponManager에게 "플레이어 본체 다 숨겨줘!" 라고 한 줄로 명령
        wm.TogglePlayerVisuals(false);

        // 2. 공격 애니메이션(이펙트) 켜기
        if (attackVisualObject != null)
        {
            attackVisualObject.SetActive(true);

            // 위치를 플레이어 본체의 중심으로 완벽히 고정!
            attackVisualObject.transform.position = wm.transform.position;

            // 마우스 방향에 맞춰서 좌우 반전
            Vector3 scale = attackVisualObject.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1f : 1f);
            attackVisualObject.transform.localScale = scale;

            Animator anim = attackVisualObject.GetComponent<Animator>();
            if (anim != null) anim.Play("Attack", 0, 0f);
        }

        // 3. 공격 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(attackDuration);

        // 4. 공격이 끝나면 이펙트를 끄고
        if (attackVisualObject != null)
        {
            attackVisualObject.SetActive(false);
        }

        // 5. [개선] WeaponManager에게 "다시 플레이어 본체 다 보여줘!" 라고 명령
        wm.TogglePlayerVisuals(true);

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastAttackPoint, lastAttackRange);
    }
}