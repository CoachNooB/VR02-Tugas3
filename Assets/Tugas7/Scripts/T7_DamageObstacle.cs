using System.Collections.Generic;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_DamageObstacle : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float hitDamage = 15f;
        [SerializeField, Min(0f)] private float cooldown = 0.75f;
        private readonly Dictionary<T7_PlayerHealth, float> nextHitTimes = new();

        public void Configure(float damage, float cooldownSeconds)
        {
            hitDamage = Mathf.Max(0f, damage);
            cooldown = Mathf.Max(0f, cooldownSeconds);
        }

        private void OnCollisionEnter(Collision collision) =>
            TryHit(collision.collider.GetComponentInParent<T7_PlayerHealth>());

        private void OnTriggerEnter(Collider other) =>
            TryHit(other.GetComponentInParent<T7_PlayerHealth>());

        public bool TryHit(T7_PlayerHealth health)
        {
            if (health == null || health.IsDead) return false;
            if (nextHitTimes.TryGetValue(health, out float next) && Time.time < next) return false;
            nextHitTimes[health] = Time.time + cooldown;
            health.ApplyDamage(hitDamage, this);
            return true;
        }
    }
}
