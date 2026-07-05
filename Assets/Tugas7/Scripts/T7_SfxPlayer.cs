using UnityEngine;

namespace Tugas7
{
    /// <summary>
    /// Player sound feedback: plays damage and respawn SFX by listening to
    /// T7_PlayerHealth events. Configured by the scene builder.
    /// </summary>
    public sealed class T7_SfxPlayer : MonoBehaviour
    {
        [SerializeField] private T7_PlayerHealth health;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip damageClip;
        [SerializeField] private AudioClip respawnClip;
        [SerializeField, Min(0f)] private float minSecondsBetweenDamage = 0.25f;
        private float lastDamageTime = -10f;

        public void Configure(T7_PlayerHealth playerHealth, AudioSource source,
            AudioClip damage, AudioClip respawn)
        {
            if (isActiveAndEnabled && health != null) Unsubscribe();
            health = playerHealth;
            audioSource = source;
            damageClip = damage;
            respawnClip = respawn;
            if (isActiveAndEnabled && health != null) Subscribe();
        }

        private void OnEnable()
        {
            if (health != null) Subscribe();
        }

        private void OnDisable()
        {
            if (health != null) Unsubscribe();
        }

        private void Subscribe()
        {
            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }

        private void Unsubscribe()
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }

        private void HandleDamaged(float amount, Object source)
        {
            // Damage can tick every frame in lava; keep a small gap between hits.
            if (Time.time - lastDamageTime < minSecondsBetweenDamage) return;
            lastDamageTime = Time.time;
            Play(damageClip);
        }

        private void HandleDied() => Play(respawnClip);

        private void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
