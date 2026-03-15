using UnityEngine;

public class MagicGauntlet : WeaponBase
{
    private int comboStep = 0;          // 0: 1타, 1: 2타, 2: 3타
    private float lastAttackTime = 0f;  // 마지막으로 공격한 시간
    private const float comboLimit = 2.0f; // 콤보 유지 시간 (2초)

    public override void ExecuteAttack(Vector2 direction, float multiplier)
    {
        WeaponManager wm = GetComponentInParent<WeaponManager>();
        if (wm == null) return;

        // 1. 시간 체크: 마지막 공격으로부터 2초가 지났다면 콤보 초기화
        if (Time.time - lastAttackTime > comboLimit)
        {
            comboStep = 0;
            Debug.Log("콤보 시간 초과! 1타부터 다시 시작합니다.");
        }

        // 마지막 공격 시간 업데이트
        lastAttackTime = Time.time;

        // 2. 스탯 가져오기 (WeaponManager에 추가한 GetCurrentPlayerMagic 활용 추천)
        float currentAtk = wm.GetCurrentPlayerAttack();
        float currentMag = wm.GetCurrentPlayerMagic();

        // 이전 답변에서 가이드드린 대로 WeaponManager를 통해 가져오거나 직접 참조
        var stats = wm.GetComponentInParent<Game.Player.PlayerStats>();
        if (stats != null) currentMag = stats.Magic.Value;

        // 3. 타수별 배율 설정
        float atkRatio = 0.5f;
        float magRatio = 0.5f;

        if (comboStep == 2) // 마지막 3타 (80% / 80%)
        {
            atkRatio = 0.8f;
            magRatio = 0.8f;
        }

        // 4. 데미지 계산 및 정수화 (차징 배율 multiplier 적용)
        int finalDamage = Mathf.RoundToInt((currentAtk * atkRatio + currentMag * magRatio) * multiplier);

        // 5. 근접 타격 판정 (OverlapsCircle)
        // 3타일 때는 주먹에 힘이 실리므로 범위를 조금 더 늘려줍니다.
        float range = (comboStep == 2) ? data.attackRange * 1.5f : data.attackRange;
        Vector2 attackPoint = (Vector2)transform.position + (direction * 0.5f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, range);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                // 적의 체력을 깎는 로직
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
            }
        }

        // 6. 다음 타수 준비 및 로그 출력
        Debug.Log($"{comboStep + 1}타 성공! (데미지: {finalDamage})");

        // 콤보 단계 증가
        comboStep++;

        // 3타를 다 쳤다면 다시 1타로 초기화
        if (comboStep > 2)
        {
            comboStep = 0;
            Debug.Log("3단 콤보 완료! 초기화됩니다.");
        }
    }
}