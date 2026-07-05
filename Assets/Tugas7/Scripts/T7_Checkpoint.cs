using UnityEngine;

namespace Tugas7
{
    [RequireComponent(typeof(Collider))]
    public sealed class T7_Checkpoint : MonoBehaviour
    {
        [SerializeField] private int checkpointIndex = 1;
        [SerializeField] private float healingPerSecond = 10f;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private T7_CourseManager courseManager;
        [SerializeField] private Renderer indicatorRenderer;
        [SerializeField] private Color inactiveColor = new(0.1f, 0.35f, 1f);
        [SerializeField] private Color activeColor = new(0.1f, 1f, 0.25f);
        [SerializeField] private AudioSource sfxSource;
        public bool IsActivated { get; private set; }

        public void SetAudio(AudioSource source) => sfxSource = source;

        // Replay support: back to inactive (blue) so it can be activated again.
        public void ResetCheckpoint()
        {
            IsActivated = false;
            SetIndicator(inactiveColor);
        }

        public void Configure(int index, T7_CourseManager manager, Transform point, Renderer indicator = null)
        {
            checkpointIndex = index;
            courseManager = manager;
            respawnPoint = point;
            indicatorRenderer = indicator;
        }

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            var health = other.GetComponentInParent<T7_PlayerHealth>();
            if (health == null || IsActivated) return;
            if (courseManager != null && courseManager.TryActivateCheckpoint(checkpointIndex, respawnPoint ?? transform))
            {
                IsActivated = true;
                SetIndicator(activeColor);
                if (sfxSource != null && sfxSource.clip != null)
                    sfxSource.PlayOneShot(sfxSource.clip);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var health = other.GetComponentInParent<T7_PlayerHealth>();
            if (health != null) HealOccupied(health, Time.fixedDeltaTime);
        }

        public void HealOccupied(T7_PlayerHealth health, float elapsed)
        {
            if (IsActivated && health != null && elapsed > 0f)
                health.Heal(healingPerSecond * elapsed);
        }

        private void SetIndicator(Color color)
        {
            if (indicatorRenderer == null) return;
            indicatorRenderer.material.color = color;
            indicatorRenderer.material.EnableKeyword("_EMISSION");
            indicatorRenderer.material.SetColor("_EmissionColor", color * 1.5f);
        }
    }
}
