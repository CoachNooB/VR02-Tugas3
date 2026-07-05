using UnityEngine;

namespace Tugas7
{
    [RequireComponent(typeof(Collider))]
    public sealed class T7_DamageVolume : MonoBehaviour
    {
        [field: SerializeField, Min(0f)] public float DamagePerSecond { get; set; } = 20f;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerStay(Collider other)
        {
            var health = other.GetComponentInParent<T7_PlayerHealth>();
            if (health != null) ApplyTo(health, Time.fixedDeltaTime);
        }

        public void ApplyTo(T7_PlayerHealth health, float elapsed)
        {
            if (health != null && elapsed > 0f)
                health.ApplyDamage(DamagePerSecond * elapsed, this);
        }
    }
}
