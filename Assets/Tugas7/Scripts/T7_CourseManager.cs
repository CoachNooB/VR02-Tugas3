using System;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_CourseManager : MonoBehaviour
    {
        [SerializeField] private T7_SpatialFeedbackUI ui;
        [SerializeField] private T7_CourseInteractable finishInteractable;
        [SerializeField] private Transform currentRespawnPoint;
        [SerializeField] private Transform initialRespawnPoint;
        public int CurrentCheckpointIndex { get; private set; }
        public Transform CurrentRespawnPoint => currentRespawnPoint;
        public bool IsRunning { get; private set; }
        public bool IsComplete { get; private set; }
        public float ElapsedTime => IsRunning ? Time.time - startTime : elapsedTime;
        public string CompletionMessage { get; private set; }
        private float startTime;
        private float elapsedTime;

        public event Action<int, Transform> CheckpointActivated;
        public event Action CourseCompleted;

        private void Awake()
        {
            if (ui == null) ui = FindAnyObjectByType<T7_SpatialFeedbackUI>();
        }

        private void Update()
        {
            if (ui != null) ui.SetTimer(ElapsedTime, IsRunning, IsComplete);
        }

        public void Configure(T7_SpatialFeedbackUI spatialUI, Transform initialRespawn)
        {
            ui = spatialUI;
            currentRespawnPoint = initialRespawn;
            initialRespawnPoint = initialRespawn;
            ui?.SetCheckpoint(0, 3);
        }

        // Replay support: put the course back into its pre-run state.
        public void ResetCourse()
        {
            IsRunning = false;
            IsComplete = false;
            CurrentCheckpointIndex = 0;
            elapsedTime = 0f;
            CompletionMessage = null;
            if (initialRespawnPoint != null) currentRespawnPoint = initialRespawnPoint;
            ui?.SetCheckpoint(0, 3);
            ui?.SetStatus("Activate the cyan Start Terminal");
        }

        public void SetFinishInteractable(T7_CourseInteractable interactable)
        {
            finishInteractable = interactable;
            if (CurrentCheckpointIndex >= 3) finishInteractable?.Unlock();
        }

        public void StartCourse()
        {
            if (IsRunning || IsComplete) return;
            IsRunning = true;
            startTime = Time.time;
            elapsedTime = 0f;
            ui?.SetStatus("Course started — reach Checkpoint 1");
        }

        public bool TryActivateCheckpoint(int index, Transform respawnPoint)
        {
            if (index != CurrentCheckpointIndex + 1 || index < 1 || index > 3) return false;
            CurrentCheckpointIndex = index;
            currentRespawnPoint = respawnPoint;
            ui?.SetCheckpoint(index, 3);
            ui?.SetStatus(index == 3 ? "Checkpoint 3 active — finish unlocked" : $"Checkpoint {index} active");
            if (index == 3) finishInteractable?.Unlock();
            CheckpointActivated?.Invoke(index, respawnPoint);
            return true;
        }

        public bool TryFinishCourse()
        {
            if (IsComplete || CurrentCheckpointIndex < 3) return false;
            if (IsRunning) elapsedTime = Time.time - startTime;
            IsRunning = false;
            IsComplete = true;
            CompletionMessage = $"Run complete — {FormatTime(elapsedTime)}";
            ui?.SetStatus($"COURSE COMPLETE — {FormatTime(elapsedTime)}");
            CourseCompleted?.Invoke();
            return true;
        }

        public static string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            return $"{minutes:00}:{seconds - minutes * 60f:00.0}";
        }
    }
}
