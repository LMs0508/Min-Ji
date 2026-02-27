using UnityEngine;
using Game.Core;
using System.Collections.Generic;

public class DashFireEnhancer : MonoBehaviour, ISkillElementEnhancer
{
    public ElementType TargetElement => ElementType.Fire;

    [Header("불 대쉬 설정")]
    public float damage = 20f;
    private bool isDashing = false;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>(); // 한 번의 대쉬에 여러 번 대미지 방지

    public void OnStart(GameObject owner)
    {
        isDashing = true;
        hitEnemies.Clear();
        Debug.Log("<color=red>[불 대쉬]</color> 화염 돌진 시작!");
    }

    public void OnUpdate(GameObject owner) { }

    public void OnEnd(GameObject owner)
    {
        isDashing = false;
        hitEnemies.Clear();
    }

    // 플레이어의 Trigger Collider를 통해 적과의 충돌을 감지합니다.
    // 플레이어 오브젝트에 Collider2D가 Is Trigger 상태로 있어야 합니다.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDashing) return;

        // 적 태그를 확인합니다 (사용 중인 적 태그에 맞춰 수정하세요)
        if (other.CompareTag("Enemy"))
        {
            // 한 번 대쉬에 중복 대미지 방지
            if (!hitEnemies.Contains(other.gameObject))
            {
                // 적의 체력 컴포넌트를 가져와 대미지를 입힙니다.
                // 몬스터 체력 스크립트 이름이 'EnemyHealth'라고 가정합니다.
                var enemyHealth = other.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    hitEnemies.Add(other.gameObject);
                    Debug.Log($"<color=orange>[불 대쉬]</color> 적에게 {damage} 대미지를 입혔습니다!");
                }
            }
        }
    }
}