using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SkillHitbox : MonoBehaviour
{
    private WeaponCharge parentSkill;
    private HashSet<Collider2D> alreadyHitEnemies = new HashSet<Collider2D>();
    private Collider2D myCollider;

    private void Awake()
    {
        parentSkill = GetComponentInParent<WeaponCharge>();
        myCollider = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        alreadyHitEnemies.Clear();
        if (myCollider != null)
        {
            List<Collider2D> overlaps = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.NoFilter(); // 일단 겹치는 건 다 가져옴

            Physics2D.OverlapCollider(myCollider, filter, overlaps);

            foreach (var hitCollider in overlaps)
            {
                CheckHit(hitCollider);
            }
        }
    }

    // 움직이다가 들어온 적 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckHit(other);
    }

    // 실제 데미지를 입히는 공통 로직
    private void CheckHit(Collider2D other)
    {
        if (parentSkill == null) return;

        // 적이고, 아직 이번 휘두름에서 때리지 않은 적이라면
        if (other.CompareTag("Enemy") && !alreadyHitEnemies.Contains(other))
        {
            alreadyHitEnemies.Add(other); // 때린 명단에 등록
            parentSkill.PerformHitboxDamage(other); // 메인 스크립트로 데미지 전달
        }
    }
}