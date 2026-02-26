using UnityEngine;
using System;

namespace Game.Player
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float baseValue;
        [SerializeField] private float bonusValue;
        [SerializeField] private float multiplier = 1f;

        public float BaseValue => baseValue;
        public float BonusValue => bonusValue;
        public float Multiplier => multiplier;

        public float Value => (baseValue + bonusValue) * multiplier;

        public void SetBase(float value) => baseValue = value;
        public void AddBonus(float value) => bonusValue += value;
        public void RemoveBonus(float value) => bonusValue -= value;
        public void Multiply(float factor) => multiplier *= factor;
        public void Divide(float factor) { if (factor != 0f) multiplier /= factor; }
    }

    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private Stat maxHP = new Stat();
        [SerializeField] private Stat maxMP = new Stat();
        [SerializeField] private Stat attack = new Stat();
        [SerializeField] private Stat magic = new Stat();
        [SerializeField] private Stat defense = new Stat(); // [추가] 방어력 스탯
        [SerializeField] private Stat moveSpeed = new Stat();
        [SerializeField] private Stat cooldownReduction = new Stat();
        [SerializeField] private Stat attackSpeed = new Stat();

        public Stat MaxHP => maxHP;
        public Stat MaxMP => maxMP;
        public Stat Attack => attack;
        public Stat Magic => magic;
        public Stat Defense => defense; // [추가] 외부 접근용 프로퍼티
        public Stat MoveSpeed => moveSpeed;
        public Stat CooldownReduction => cooldownReduction;
        public Stat AttackSpeed => attackSpeed;

        [Header("Runtime")]
        [SerializeField] private float currentHP;
        [SerializeField] private float currentMP;

        public float CurrentHP => currentHP;
        public float CurrentMP => currentMP;

        public event Action<float, float> OnHPChanged;
        public event Action<float, float> OnMPChanged;

        private void Awake()
        {
            currentHP = MaxHP.Value;
            currentMP = MaxMP.Value;
            ClampResources();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            currentHP = Mathf.Min(currentHP + amount, MaxHP.Value);
            OnHPChanged?.Invoke(currentHP, MaxHP.Value);
        }

        // [수정] 방어력 로직이 적용된 데미지 계산
        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;

            // 방어력 계산: 1당 1% 감소 (최대 100% 감쇄로 제한)
            // 예: Defense.Value가 10이면 reduction은 0.1(10%)
            float reductionPercent = Mathf.Clamp(Defense.Value, 0f, 100f) * 0.01f;

            // 최종 데미지 = 원래 데미지 * (1 - 감쇄율)
            // 예: 100 * (1 - 0.1) = 90 데미지
            float finalDamage = amount * (1f - reductionPercent);

            currentHP = Mathf.Max(currentHP - finalDamage, 0f);
            OnHPChanged?.Invoke(currentHP, MaxHP.Value);

            Debug.Log($"원래 데미지: {amount}, 방어 적용 후: {finalDamage} (방어력: {Defense.Value})");
        }

        public bool SpendMP(float amount)
        {
            if (amount <= 0f) return true;
            if (currentMP < amount) return false;

            currentMP -= amount;
            OnMPChanged?.Invoke(currentMP, MaxMP.Value);
            return true;
        }

        public void RestoreMP(float amount)
        {
            if (amount <= 0f) return;
            currentMP = Mathf.Min(currentMP + amount, MaxMP.Value);
            OnMPChanged?.Invoke(currentMP, MaxMP.Value);
        }

        public float ApplyCooldownReduction(float baseCooldownSeconds)
        {
            float cdr = Mathf.Clamp(cooldownReduction.Value, 0f, 0.9f);
            return baseCooldownSeconds * (1f - cdr);
        }

        public void ClampResources()
        {
            currentHP = Mathf.Clamp(currentHP, 0f, MaxHP.Value);
            currentMP = Mathf.Clamp(currentMP, 0f, MaxMP.Value);

            OnHPChanged?.Invoke(currentHP, MaxHP.Value);
            OnMPChanged?.Invoke(currentMP, MaxMP.Value);
        }
    }
}