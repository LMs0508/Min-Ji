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
        [Header("UI & Effect")]
        public GameObject damageTextPrefab;
        public Transform popupPoint;

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

            SpawnDamageText(amount);
        }

        private void SpawnDamageText(float damage)
        {
            if (damageTextPrefab == null) return;

            // 1. 위치 설정
            Vector3 spawnPos = (popupPoint != null) ? popupPoint.position : transform.position + Vector3.up * 1.5f;

            // 2. [핵심 수정] 생성할 때 세 번째 인자로 popupPoint를 넣어 부모로 설정합니다.
            // 이렇게 해야 UI가 Canvas(부모)의 영향을 받아 화면에 그려집니다.
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity, popupPoint);

            // 3. 프리팹이 꺼져있을 경우를 대비해 깨우기
            textObj.SetActive(true);

            DamageText_Player damageScript = textObj.GetComponent<DamageText_Player>();
            if (damageScript != null)
            {
                damageScript.Setup(damage);
            }
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
            // cooldownReduction.Value 0.2 == 20%인지
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
