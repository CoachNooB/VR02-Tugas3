using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tugas7
{
    [RequireComponent(typeof(Canvas))]
    public sealed class T7_SpatialFeedbackUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text checkpointText;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Image feedbackPanel;
        [SerializeField] private T7_PlayerHealth playerHealth;
        [SerializeField] private Transform followCamera;
        [SerializeField] private float followDistance = 1.2f;
        [SerializeField] private Vector2 offset = new(-0.34f, 0.27f);
        [SerializeField] private float feedbackDuration = 0.18f;
        [SerializeField] private Color idlePanelColor = new(0.02f, 0.03f, 0.05f, 0.9f);
        private float feedbackUntil;
        public Color CurrentFeedbackColor { get; private set; } = Color.clear;
        public Vector2 FollowOffset => offset;

        private void Awake()
        {
            EnsureWorldSpace();
            if (followCamera == null && Camera.main != null) followCamera = Camera.main.transform;
        }

        private void OnEnable()
        {
            Subscribe(playerHealth);
        }

        private void OnDisable()
        {
            Unsubscribe(playerHealth);
        }

        private void Update()
        {
            if (feedbackPanel != null && Time.unscaledTime >= feedbackUntil)
            {
                feedbackPanel.color = idlePanelColor;
                CurrentFeedbackColor = Color.clear;
            }
        }

        private void LateUpdate()
        {
            if (followCamera == null) return;
            transform.position = followCamera.position + followCamera.forward * followDistance +
                                 followCamera.right * offset.x + followCamera.up * offset.y;
            transform.rotation = followCamera.rotation;
        }

        public void Configure(TMP_Text hp, Image fill, TMP_Text checkpoint, TMP_Text prompt,
            TMP_Text status, TMP_Text timer, Image panel, Transform camera)
        {
            healthText = hp; healthFill = fill; checkpointText = checkpoint; promptText = prompt;
            statusText = status; timerText = timer; feedbackPanel = panel; followCamera = camera;
            if (feedbackPanel != null) idlePanelColor = feedbackPanel.color;
            EnsureWorldSpace();
        }

        public void ConfigurePlacement(float distance, Vector2 followOffset)
        {
            followDistance = Mathf.Max(0.1f, distance);
            offset = followOffset;
        }

        public void Bind(T7_PlayerHealth health)
        {
            if (isActiveAndEnabled) Unsubscribe(playerHealth);
            playerHealth = health;
            if (isActiveAndEnabled) Subscribe(playerHealth);
            SetHealth(health.CurrentHealth, health.MaxHealth);
        }

        private void Subscribe(T7_PlayerHealth health)
        {
            if (health == null) return;
            health.HealthChanged -= SetHealth;
            health.Damaged -= HandleDamaged;
            health.Healed -= ShowHealingFeedback;
            health.HealthChanged += SetHealth;
            health.Damaged += HandleDamaged;
            health.Healed += ShowHealingFeedback;
            SetHealth(health.CurrentHealth, health.MaxHealth);
        }

        private void Unsubscribe(T7_PlayerHealth health)
        {
            if (health == null) return;
            health.HealthChanged -= SetHealth;
            health.Damaged -= HandleDamaged;
            health.Healed -= ShowHealingFeedback;
        }

        private void HandleDamaged(float amount, Object _) => ShowDamageFeedback(amount);

        public void EnsureWorldSpace() => GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        public void SetHealth(float current, float maximum)
        {
            if (healthText != null) healthText.text = $"HP {Mathf.CeilToInt(current)}/{Mathf.CeilToInt(maximum)}";
            if (healthFill != null) healthFill.fillAmount = maximum > 0f ? current / maximum : 0f;
        }
        public void SetCheckpoint(int current, int total) { if (checkpointText != null) checkpointText.text = $"CHECKPOINT {current}/{total}"; }
        public void SetInteractionPrompt(string prompt) { if (promptText != null) promptText.text = prompt; }
        public void SetStatus(string status) { if (statusText != null) statusText.text = status; }
        public void SetTimer(float seconds, bool running, bool complete)
        {
            if (timerText != null) timerText.text = $"{(complete ? "FINISH" : running ? "TIME" : "READY")}  {T7_CourseManager.FormatTime(seconds)}";
        }
        public void ShowDamageFeedback(float _) => SetFeedback(new Color(0.85f, 0.05f, 0.03f, 0.12f));
        public void ShowHealingFeedback(float _) => SetFeedback(new Color(0.05f, 0.8f, 0.15f, 0.08f));
        private void SetFeedback(Color color)
        {
            CurrentFeedbackColor = color;
            feedbackUntil = Time.unscaledTime + feedbackDuration;
            if (feedbackPanel != null) feedbackPanel.color = color;
        }
    }
}
