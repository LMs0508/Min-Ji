using Game.Player;
using UnityEngine;

public class BossAttackEvents : MonoBehaviour
{
    private PlayerDebuffManager playerDebuff;
    public float damage = 20f;

    void Start()
    {
        playerDebuff = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDebuffManager>();
    }

    // 애니메이션 이벤트에서 호출 (Attack1 ~ Attack5)
    public void OnBossAttack(int attackType)
    {
        if (playerDebuff == null) return;

        switch (attackType)
        {
            case 1: // 1페이즈: 내려치기 -> 슬로우
                ApplyEffect(DebuffType.Slow, 0.5f, 3f);
                break;
            case 2: // 1페이즈: 눈빛 발사 -> 실명
                ApplyEffect(DebuffType.Blind, 0.8f, 2f);
                break;
            case 3: // 1페이즈: 포효 -> 약화(공격력 감소)
                ApplyEffect(DebuffType.Weakness, 0.7f, 4f);
                break;
            case 4: // 2페이즈 추가: 저주의 숨결 -> 화상
                ApplyEffect(DebuffType.Burn, 5f, 5f);
                break;
            case 5: // 2페이즈 추가: 석화 시선 -> 스턴(속박)
                ApplyEffect(DebuffType.Stun, 0f, 1.5f);
                break;
        }
    }

    private void ApplyEffect(DebuffType type, float power, float duration)
    {
        playerDebuff.ApplyDebuff(type, power, duration);
        // 체력 감소도 함께 적용
        playerDebuff.GetComponent<PlayerStats>().TakeDamage(damage);
    }
}