using System.Collections;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_RespawnController : MonoBehaviour
    {
        [SerializeField] private T7_PlayerHealth health;
        [SerializeField] private Rigidbody body;
        [SerializeField] private T7_CourseManager courseManager;
        [SerializeField] private Behaviour movementController;
        [SerializeField] private float respawnDelay = 1f;
        [SerializeField] private float respawnHealth = 50f;

        private void Awake()
        {
            if (health == null) health = GetComponent<T7_PlayerHealth>();
            if (body == null) body = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            if (health != null) health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= HandleDeath;
        }

        public void Configure(T7_PlayerHealth playerHealth, Rigidbody playerBody,
            T7_CourseManager manager, float delay, float restoredHealth, Behaviour controller = null)
        {
            if (isActiveAndEnabled && health != null) health.Died -= HandleDeath;
            health = playerHealth;
            body = playerBody;
            courseManager = manager;
            respawnDelay = Mathf.Max(0f, delay);
            respawnHealth = restoredHealth;
            movementController = controller;
            if (isActiveAndEnabled && health != null) health.Died += HandleDeath;
        }

        private void HandleDeath()
        {
            if (isActiveAndEnabled) StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            health.BeginRespawn();
            if (movementController != null) movementController.enabled = false;
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }
            if (respawnDelay > 0f) yield return new WaitForSeconds(respawnDelay);
            Transform point = courseManager != null ? courseManager.CurrentRespawnPoint : null;
            if (point != null) transform.SetPositionAndRotation(point.position, point.rotation);
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = false;
            }
            health.RestoreForRespawn(respawnHealth);
            if (movementController != null) movementController.enabled = true;
        }
    }
}
