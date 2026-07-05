using UnityEngine;

namespace Tugas7
{
    [RequireComponent(typeof(Collider))]
    public sealed class T7_ResetVolume : MonoBehaviour
    {
        private void Reset() => GetComponent<Collider>().isTrigger = true;
        private void OnTriggerEnter(Collider other)
        {
            var health = other.GetComponentInParent<T7_PlayerHealth>();
            if (health != null) health.ApplyDamage(health.MaxHealth, this);
        }
    }
}
