using UnityEngine;
using Game.Player;

public class MeleeArea : MonoBehaviour
{
    [Header("공격 판정 설정")]
    public float radius = 0.5f;
    public string targetTag = "Player";

    private float damage;

    public void CheckAttack(float attackDamage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag(targetTag))
            {
                PlayerStats pStats = hit.GetComponent<PlayerStats>();
                if (pStats == null) pStats = hit.GetComponentInChildren<PlayerStats>();

                if (pStats != null)
                {
                    pStats.TakeDamage(attackDamage);
                    return;
                }
            }
        }
    }

    public void OnMonsterHit(float attackDamage)
    {
        CheckAttack(attackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}