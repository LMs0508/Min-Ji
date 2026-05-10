using UnityEngine;

namespace Game.Player
{
    public class PlayerPoisonDebuff : MonoBehaviour
    {
        private float durationTimer = 0f;
        private float tickDamage = 0f;
        private float tickInterval = 0.5f; // 0.5초마다 데미지
        private float nextTickTime = 0f;
        private PlayerStats stats;

        void Awake()
        {
            // 플레이어 스탯 스크립트 찾기
            stats = GetComponent<PlayerStats>() ?? GetComponentInParent<PlayerStats>() ?? transform.root.GetComponentInChildren<PlayerStats>();
        }

        // 독 디버프 적용 (지속시간, 틱당 데미지)
        public void ApplyPoison(float duration, float damagePerTick)
        {
            // 이미 독에 걸려있다면 지속시간을 다시 갱신
            durationTimer = Mathf.Max(durationTimer, duration);
            tickDamage = damagePerTick;
        }

        void Update()
        {
            if (durationTimer > 0)
            {
                durationTimer -= Time.deltaTime;
                if (Time.time > nextTickTime)
                {
                    nextTickTime = Time.time + tickInterval;
                    // 플레이어 스탯 게임 오브젝트가 켜져 있을 때(살아있을 때)만 데미지 적용
                    if (stats != null && stats.gameObject.activeInHierarchy)
                    {
                        stats.TakeDamage(tickDamage);
                    }
                }
            }
            else { Destroy(this); } // 지속시간이 끝나면 스크립트 삭제
        }
    }
}