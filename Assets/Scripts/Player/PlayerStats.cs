using UnityEngine;
using System;

namespace Game.Player
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float baseValue;
        [SerializeField] private float bonusValue;      // 아이템/레벨업으로 더해지는 값
        [SerializeField] private float multiplier = 1f; // 버프/디버프 배율

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
        [SerializeField] private Stat moveSpeed = new Stat();
        [SerializeField] private Stat cooldownReduction = new Stat(); // % (0~0.8 같은 식으로 쓰는 걸 추천)
        [SerializeField] private Stat attackSpeed = new Stat();      // 배율(1.0=기본)

        public Stat MaxHP => maxHP;
        public Stat MaxMP => maxMP;
        public Stat Attack => attack;
        public Stat Magic => magic;
        public Stat MoveSpeed => moveSpeed;
        public Stat CooldownReduction => cooldownReduction;
        public Stat AttackSpeed => attackSpeed;

        [Header("Runtime")]
        [SerializeField] private float currentHP;
        [SerializeField] private float currentMP;

        public float CurrentHP => currentHP;
        public float CurrentMP => currentMP;

        public event Action<float, float> OnHPChanged; // (current, max)
        public event Action<float, float> OnMPChanged;

        private void Awake()
        {
            // 시작 시 풀로 채워두기
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

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;
            currentHP = Mathf.Max(currentHP - amount, 0f);
            OnHPChanged?.Invoke(currentHP, MaxHP.Value);
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
            // cooldownReduction.Value를 0~0.9 정도로 제한 추천
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
