using System.Collections;
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
        [Header("UI & Effect")]
        public GameObject damageTextPrefab;
        public Transform popupPoint;
        [SerializeField] private float hitFlashDuration = 0.15f;

        private SpriteRenderer sr;
        private Coroutine hitFlashCoroutine;

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

        [Header("Death Settings")]
        public GameObject deathPrefab;

        // [추가] 플레이어 사망 여부를 확인하는 프로퍼티
        public bool IsDead { get; private set; }

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
            if (IsDead) return; // 죽었으면 재생하지 않음
            RegenerateStats();
        }

        private void RegenerateStats()
        {
            bool hpChanged = false;
            bool mpChanged = false;

            if (currentHP < MaxHP.Value && HPRegen.Value > 0)
            {
                float regenAmount = MaxHP.Value * (HPRegen.Value * 0.01f) * Time.deltaTime;
                currentHP = Mathf.Min(currentHP + regenAmount, MaxHP.Value);
                hpChanged = true;
            }

            if (currentMP < MaxMP.Value && MPRegen.Value > 0)
            {
                float regenAmount = MaxMP.Value * (MPRegen.Value * 0.01f) * Time.deltaTime;
                currentMP = Mathf.Min(currentMP + regenAmount, MaxMP.Value);
                mpChanged = true;
            }

            if (hpChanged) OnHPChanged?.Invoke(currentHP, MaxHP.Value);
            if (mpChanged) OnMPChanged?.Invoke(currentMP, MaxMP.Value);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;
            currentHP = Mathf.Min(currentHP + amount, MaxHP.Value);
            OnHPChanged?.Invoke(currentHP, MaxHP.Value);
        }

        public void RestoreMana(float amount)
        {
            if (amount <= 0f || IsDead) return;
            currentMP = Mathf.Min(currentMP + amount, maxMP.Value);
            OnMPChanged?.Invoke(currentMP, MaxMP.Value);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || IsDead) return; // 이미 죽었으면 데미지 무시

            float reductionPercent = Mathf.Clamp(Defense.Value, 0f, 100f) * 0.01f;
            float finalDamage = amount * (1f - reductionPercent);
            currentHP = Mathf.Max(currentHP - finalDamage, 0f);
            OnHPChanged?.Invoke(currentHP, MaxHP.Value);

            SpawnDamageText(amount);

            // [추가] 체력이 0이 되면 사망 처리 실행
            if (currentHP <= 0f)
            {
                Die();
                return; // 사망 시 번쩍임 효과 무시
            }

            if (hitFlashCoroutine != null)
            {
                StopCoroutine(hitFlashCoroutine);
            }
            ForceResetVisual();

            hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            StopAllCoroutines();
            ForceResetVisual();

            // 1. 물리 및 콜라이더 즉시 정지
            Rigidbody2D rb = transform.root.GetComponentInChildren<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            Collider2D[] colliders = transform.root.GetComponentsInChildren<Collider2D>();
            foreach (var col in colliders) col.enabled = false;

            // 2. [핵심] 기존의 모든 비주얼(몸체, 무기 등)을 완전히 숨김
            // Shadow를 제외한 모든 자식 오브젝트를 비활성화합니다.
            foreach (Transform child in transform.root)
            {
                if (child.name != "Shadow")
                {
                    child.gameObject.SetActive(false);
                }
            }

            // 3. [핵심] 사망 프리팹을 현재 위치에 생성
            if (deathPrefab != null)
            {
                // 플레이어의 현재 위치와 방향에 맞춰 생성
                GameObject deathEffect = Instantiate(deathPrefab, transform.position, transform.rotation);

                // 생성된 프리팹이 마지막 프레임에서 멈추도록 설정되어 있는지 확인
                Animator anim = deathEffect.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Play("Die", 0, 0f);
                }
            }

            // 4. 다른 모든 스크립트 비활성화
            MonoBehaviour[] scripts = transform.root.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script == this || script == null) continue;
                if (script.GetType().Name.Contains("DamageText")) continue;
                script.enabled = false;
            }

            Debug.Log("<color=red>플레이어 사망: 사망 프리팹 생성 완료</color>");
        }

        private IEnumerator HitFlashRoutine()
        {
            SpriteRenderer[] allSrs = transform.root.GetComponentsInChildren<SpriteRenderer>(true);

            if (allSrs != null && allSrs.Length > 0)
            {
                foreach (var sr in allSrs)
                {
                    if (sr != null && sr.gameObject.activeInHierarchy && sr.gameObject.name != "Shadow")
                    {
                        sr.color = Color.red;
                    }
                }

                yield return new WaitForSeconds(hitFlashDuration);

                foreach (var sr in allSrs)
                {
                    if (sr != null) sr.color = Color.white;
                }
            }

            hitFlashCoroutine = null;
        }

        public void ForceResetVisual()
        {
            foreach (var sr in transform.root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr != null) sr.color = Color.white;
            }
        }

        private void OnDisable()
        {
            ResetAllColors();
        }

        public void ResetAllColors()
        {
            SpriteRenderer[] allSrs = transform.root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in allSrs)
            {
                if (sr != null) sr.color = Color.white;
            }
        }

        private void SpawnDamageText(float damage)
        {
            if (damageTextPrefab == null) return;

            Vector3 spawnPos = (popupPoint != null) ? popupPoint.position : transform.position + Vector3.up * 1.5f;
            GameObject textObj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity, popupPoint);
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
            if (currentMP < amount || IsDead) return false;
            currentMP -= amount;
            OnMPChanged?.Invoke(currentMP, MaxMP.Value);
            return true;
        }

        public void RestoreMP(float amount)
        {
            if (amount <= 0f || IsDead) return;
            currentMP = Mathf.Min(currentMP + amount, MaxMP.Value);
            OnMPChanged?.Invoke(currentMP, MaxMP.Value);
        }

        public float ApplyCooldownReduction(float baseCooldownSeconds)
        {
            float cdr = Mathf.Clamp(cooldownReduction.Value, 0f, 0.9f);
            return baseCooldownSeconds * (1f - cdr);
        }

        private Coroutine speedBuffCoroutine;
        private Coroutine attackBuffCoroutine;
        private Coroutine defenseBuffCoroutine;

        public void ApplySpeedBuff(float multiplier, float duration)
        {
            if (IsDead) return;
            if (speedBuffCoroutine != null)
            {
                StopCoroutine(speedBuffCoroutine);
                moveSpeed.Divide(multiplier);
            }
            speedBuffCoroutine = StartCoroutine(SpeedBuffRoutine(multiplier, duration));
        }

        private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
        {
            moveSpeed.Multiply(multiplier);
            yield return new WaitForSeconds(duration);
            moveSpeed.Divide(multiplier);
            speedBuffCoroutine = null;
        }

        public void ApplyAttackBuff(float multiplier, float duration)
        {
            if (IsDead) return;
            if (attackBuffCoroutine != null)
            {
                StopCoroutine(attackBuffCoroutine);
                attack.Divide(multiplier);
            }
            attackBuffCoroutine = StartCoroutine(AttackBuffRoutine(multiplier, duration));
        }

        private IEnumerator AttackBuffRoutine(float multiplier, float duration)
        {
            attack.Multiply(multiplier);
            yield return new WaitForSeconds(duration);
            attack.Divide(multiplier);
            attackBuffCoroutine = null;
        }

        public void ApplyDefenseBuff(float multiplier, float duration)
        {
            if (IsDead) return;
            if (defenseBuffCoroutine != null)
            {
                StopCoroutine(defenseBuffCoroutine);
                defense.Divide(multiplier);
            }
            defenseBuffCoroutine = StartCoroutine(DefenseBuffRoutine(multiplier, duration));
        }

        private IEnumerator DefenseBuffRoutine(float multiplier, float duration)
        {
            defense.Multiply(multiplier);
            yield return new WaitForSeconds(duration);
            defense.Divide(multiplier);
            defenseBuffCoroutine = null;
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