using System;
using UnityEngine;
using UnityEngine.Events;

namespace Tugas7
{
    public sealed class T7_CourseInteractable : MonoBehaviour
    {
        public enum CourseAction { Message, StartCourseAndOpenGate, ReportPressurePlate, FinishCourse }
        [SerializeField] private string displayName = "Interactable";
        [SerializeField] private string successMessage = "Activated";
        [SerializeField] private string lockedMessage = "Locked";
        [SerializeField] private bool startsLocked;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color glowColor = Color.cyan;
        [SerializeField, Min(1f)] private float highlightIntensity = 4f;
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private CourseAction courseAction;
        [SerializeField] private T7_CourseManager courseManager;
        [SerializeField] private T7_Gate gate;
        [SerializeField] private T7_PressurePlate pressurePlate;
        private bool unlocked;
        private MaterialPropertyBlock properties;
        private MaterialPropertyBlock propertiesBeforeHighlight;
        private bool isHighlighted;
        public string DisplayName => displayName;
        public bool IsUnlocked => unlocked ||
            (courseAction == CourseAction.FinishCourse && courseManager != null &&
             courseManager.CurrentCheckpointIndex >= 3);
        public event Func<string> InteractionRequested;

        private void Awake()
        {
            unlocked = !startsLocked;
            if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
            properties = new MaterialPropertyBlock();
        }

        public void Configure(string name, string message, bool locked = false, Renderer renderer = null)
        {
            displayName = name;
            successMessage = message;
            startsLocked = locked;
            unlocked = !locked;
            if (renderer != null)
                targetRenderer = renderer;
        }

        public void ConfigureHighlight(Color color, float intensity)
        {
            glowColor = color;
            highlightIntensity = Mathf.Max(1f, intensity);
        }

        public void ConfigureAction(CourseAction action, T7_CourseManager manager,
            T7_Gate controlledGate = null, T7_PressurePlate plate = null)
        {
            courseAction = action;
            courseManager = manager;
            gate = controlledGate;
            pressurePlate = plate;
        }

        public string Prompt => IsUnlocked ? $"Press E — {displayName}" : $"{displayName} — LOCKED";

        public void SetHighlighted(bool highlighted)
        {
            if (targetRenderer == null) return;
            if (highlighted == isHighlighted) return;
            properties ??= new MaterialPropertyBlock();
            propertiesBeforeHighlight ??= new MaterialPropertyBlock();
            if (highlighted)
            {
                targetRenderer.GetPropertyBlock(propertiesBeforeHighlight);
                targetRenderer.GetPropertyBlock(properties);
                properties.SetColor("_EmissionColor", glowColor * highlightIntensity);
                targetRenderer.SetPropertyBlock(properties);
            }
            else
            {
                targetRenderer.SetPropertyBlock(propertiesBeforeHighlight);
            }
            isHighlighted = highlighted;
        }

        private void OnDisable() => SetHighlighted(false);

        public string Interact()
        {
            if (!IsUnlocked) return lockedMessage;
            onInteract?.Invoke();
            switch (courseAction)
            {
                case CourseAction.StartCourseAndOpenGate:
                    courseManager?.StartCourse();
                    gate?.Open();
                    return successMessage;
                case CourseAction.ReportPressurePlate:
                    return pressurePlate != null && pressurePlate.IsPressed
                        ? "Pressure plate active — gate open"
                        : "Place the yellow crate on the purple plate";
                case CourseAction.FinishCourse:
                    if (courseManager != null && courseManager.IsComplete)
                        return courseManager.CompletionMessage;
                    return courseManager != null && courseManager.TryFinishCourse()
                        ? courseManager.CompletionMessage
                        : "Finish locked — activate all checkpoints";
            }
            if (InteractionRequested != null)
            {
                foreach (Func<string> handler in InteractionRequested.GetInvocationList())
                {
                    string result = handler();
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            }
            return successMessage;
        }

        public void Unlock() => unlocked = true;
    }
}
