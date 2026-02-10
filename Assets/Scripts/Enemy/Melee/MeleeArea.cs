using UnityEngine;

public class MeleeArea : MonoBehaviour
{
    [Header("공격 판정 설정")]
    public float radius = 0.5f;
    public LayerMask targetLayer; // 누구를 공격할지(Player)

    private int damage;

    public void CheckAttack(int attackDamage)
    {
        damage = attackDamage;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, targetLayer);
        
        if(hit != null)
        {
            Debug.Log($"{hit.name}에게 {damage} 데미지를 입혔습니다!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
