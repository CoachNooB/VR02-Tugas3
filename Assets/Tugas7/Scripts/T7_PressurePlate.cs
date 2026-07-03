using System;
using UnityEngine;

namespace Tugas7
{
    [RequireComponent(typeof(Collider))]
    public sealed class T7_PressurePlate : MonoBehaviour
    {
        [SerializeField] private Rigidbody designatedCrate;
        [SerializeField] private Renderer plateRenderer;
        [SerializeField] private Color offColor = new(0.45f, 0.1f, 0.65f);
        [SerializeField] private Color onColor = new(0.1f, 0.9f, 0.2f);
        [SerializeField] private AudioSource sfxSource;
        private int overlapCount;

        public void SetAudio(AudioSource source) => sfxSource = source;
        public bool IsPressed { get; private set; }
        public event Action Pressed;
        public event Action Released;

        private void Reset() => GetComponent<Collider>().isTrigger = true;
        public void SetDesignatedCrate(Rigidbody crate) => designatedCrate = crate;
        public void SetRenderer(Renderer renderer) => plateRenderer = renderer;

        private void OnTriggerEnter(Collider other) => EvaluateBody(other.attachedRigidbody, true);
        private void OnTriggerExit(Collider other) => EvaluateBody(other.attachedRigidbody, false);

        public void EvaluateBody(Rigidbody body, bool entered)
        {
            if (body == null || body != designatedCrate) return;
            overlapCount = Mathf.Max(0, overlapCount + (entered ? 1 : -1));
            SetPressed(overlapCount > 0);
        }

        private void SetPressed(bool pressed)
        {
            if (IsPressed == pressed) return;
            IsPressed = pressed;
            if (plateRenderer != null)
            {
                Color color = pressed ? onColor : offColor;
                plateRenderer.material.color = color;
                plateRenderer.material.SetColor("_EmissionColor", color);
            }
            if (sfxSource != null && sfxSource.clip != null)
                sfxSource.PlayOneShot(sfxSource.clip);
            if (pressed) Pressed?.Invoke(); else Released?.Invoke();
        }
    }
}
