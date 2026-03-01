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
        [SerializeField] private Stat hpRegen = new Stat(); // 1 = 최대 체력의 1% 회복
        [SerializeField] private Stat mpRegen = new Stat(); // 2 = 최대 마나의 2% 회복
        [SerializeField] private Stat attack = new Stat();
        [SerializeField] private Stat magic = new Stat();
        [SerializeField] private Stat defense = new Stat();
        [SerializeField] private Stat moveSpeed = new Stat();
        [SerializeField] private Stat cooldownReduction = new Stat();
        [SerializeField] private Stat attackSpeed = new Stat();

        public Stat MaxHP => maxHP;
        public Stat MaxMP => maxMP;
        public Stat HPRegen => hpRegen;
        public Stat MPRegen => mpRegen;
        public Stat Attack => attack;
        public Stat Magic => magic;
        public Stat Defense => defense;
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

        private void Update()
        {
            RegenerateStats();
        }

        private void RegenerateStats()
        {
            // [수정] 퍼센트 기반 체력 재생 로직
            if (currentHP < MaxHP.Value && HPRegen.Value > 0)
            {
                // 공식: 최대 체력 * (재생 스탯 * 0.01) * 초당 시간
                float regenAmount = MaxHP.Value * (HPRegen.Value * 0.01f) * Time.deltaTime;
                currentHP = Mathf.Min(currentHP + regenAmount, MaxHP.Value);
                OnHPChanged?.Invoke(currentHP, MaxHP.Value);
            }

            // [수정] 퍼센트 기반 마나 재생 로직
            if (currentMP < MaxMP.Value && MPRegen.Value > 0)
            {
                // 공식: 최대 마나 * (재생 스탯 * 0.01) * 초당 시간
                float regenAmount = MaxMP.Value * (MPRegen.Value * 0.01f) * Time.deltaTime;
                currentMP = Mathf.Min(currentMP + regenAmount, MaxMP.Value);
                OnMPChanged?.Invoke(currentMP, MaxMP.Value);
            }
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
            float reductionPercent = Mathf.Clamp(Defense.Value, 0f, 100f) * 0.01f;
            float finalDamage = amount * (1f - reductionPercent);
            currentHP = Mathf.Max(currentHP - finalDamage, 0f);
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