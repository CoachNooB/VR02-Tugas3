using System.Collections.Generic;
using UnityEngine;

namespace Tugas7
{
    public sealed class T7_FinishPresentation : MonoBehaviour
    {
        [SerializeField] private T7_CourseManager courseManager;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<T7_TutorialNPC> finishNpcs = new();

        private bool subscribed;
        private bool presented;
        private bool missingClipWarningLogged;

        public int PlayCount { get; private set; }

        public void Configure(T7_CourseManager manager, AudioSource source,
            IReadOnlyList<T7_TutorialNPC> npcs)
        {
            Unsubscribe();
            courseManager = manager;
            audioSource = source;
            finishNpcs.Clear();
            if (npcs != null)
                for (int i = 0; i < npcs.Count; i++)
                    finishNpcs.Add(npcs[i]);
            Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (!isActiveAndEnabled || subscribed || courseManager == null)
                return;
            courseManager.CourseCompleted += Present;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
                return;
            if (courseManager != null)
                courseManager.CourseCompleted -= Present;
            subscribed = false;
        }

        // Replay support: allow the celebration to play again on the next finish.
        public void ResetPresentation() => presented = false;

        private void Present()
        {
            if (presented)
                return;

            presented = true;
            PlayCount++;
            if (audioSource != null)
            {
                if (audioSource.clip != null)
                    audioSource.PlayOneShot(audioSource.clip);
                else if (!missingClipWarningLogged)
                {
                    missingClipWarningLogged = true;
                    Debug.LogWarning("Finish presentation AudioSource has no clip.", this);
                }
            }

            for (int i = 0; i < finishNpcs.Count; i++)
                finishNpcs[i]?.EnterVictory();
        }
    }
}
