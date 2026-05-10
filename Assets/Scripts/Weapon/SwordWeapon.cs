using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordWeapon : WeaponBase
{
    [Header("�� ���־� & �ִϸ��̼�")]
    public GameObject attackVisualObject;

    [Tooltip("���� �ִϸ��̼��� �����Ǵ� �ð�(��)�� �Է��ϼ���.")]
    public float attackDuration = 0.5f;

    private Vector2 lastAttackPoint;
    private float lastAttackRange;

    // ���� ������ üũ�ؼ� ��Ÿ ����
    private bool isAttacking = false;

    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        // �̹� ���� ���̸� �ߺ� ���� ����
        if (isAttacking) return;

        WeaponManager wm = GetComponentInParent<WeaponManager>();
        float playerAtk = (wm != null) ? wm.GetCurrentPlayerAttack() : 0;
        float finalDamage = playerAtk * 1.0f;

        // 1. ������ ���� ���� ����
        Vector2 attackPoint = (Vector2)transform.position + (direction * 0.5f);
        float range = data.attackRange;

        lastAttackPoint = attackPoint;
        lastAttackRange = range;

        // 2. ������ ����
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, range);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Debug.Log($"{enemy.name}���� {finalDamage}�� ���� ������!");
                    enemy.GetComponentInParent<EnemyHealth>()?.TakeDamage(finalDamage);
            }
        }

        // 3. [�ٽ�] ���� �ִϸ��̼� �� ���־� ��ü �ڷ�ƾ ����
        if (wm != null)
        {
            StartCoroutine(AttackRoutine(wm, direction));
        }
    }

    private IEnumerator AttackRoutine(WeaponManager wm, Vector2 direction)
    {
        isAttacking = true;

        // 1. [����] WeaponManager���� "�÷��̾� ��ü �� ������!" ���� �� �ٷ� ����
        wm.TogglePlayerVisuals(false);

        // 2. ���� �ִϸ��̼�(����Ʈ) �ѱ�
        if (attackVisualObject != null)
        {
            attackVisualObject.SetActive(true);

            // ��ġ�� �÷��̾� ��ü�� �߽����� �Ϻ��� ����!
            attackVisualObject.transform.position = wm.transform.position;

            // ���콺 ���⿡ ���缭 �¿� ����
            Vector3 scale = attackVisualObject.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x < 0 ? -1f : 1f);
            attackVisualObject.transform.localScale = scale;

            Animator anim = attackVisualObject.GetComponent<Animator>();
            if (anim != null) anim.Play("Attack", 0, 0f);
        }

        // 3. ���� �ִϸ��̼� ���̸�ŭ ����
        yield return new WaitForSeconds(attackDuration);

        // 4. ������ ������ ����Ʈ�� ����
        if (attackVisualObject != null)
        {
            attackVisualObject.SetActive(false);
        }

        // 5. [����] WeaponManager���� "�ٽ� �÷��̾� ��ü �� ������!" ���� ����
        wm.TogglePlayerVisuals(true);

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastAttackPoint, lastAttackRange);
    }
}