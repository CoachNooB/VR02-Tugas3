using System;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_PlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }
        public bool IsRespawning { get; private set; }

        public event Action<float, float> HealthChanged;
        public event Action<float, UnityEngine.Object> Damaged;
        public event Action<float> Healed;
        public event Action Died;

        private void Awake()
        {
            maxHealth = Mathf.Max(1f, maxHealth);
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            IsDead = currentHealth <= 0f;
        }

        public void ApplyDamage(float amount, UnityEngine.Object source)
        {
            if (amount <= 0f || IsDead || IsRespawning) return;
            float previous = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            float applied = previous - currentHealth;
            if (applied <= 0f) return;
            Damaged?.Invoke(applied, source);
            HealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth > 0f) return;
            IsDead = true;
            Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead || IsRespawning) return;
            float previous = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            float applied = currentHealth - previous;
            if (applied <= 0f) return;
            Healed?.Invoke(applied);
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void BeginRespawn() => IsRespawning = true;

        public void RestoreForRespawn(float amount)
        {
            currentHealth = Mathf.Clamp(amount, 0f, maxHealth);
            IsDead = false;
            IsRespawning = false;
            HealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
