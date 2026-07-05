using UnityEngine;

namespace Tugas7
{
    /// <summary>
    /// Plays a short voice blip whenever the tutorial NPC advances to a new
    /// dialogue line. Configured by the scene builder.
    /// </summary>
    public sealed class T7_NpcVoice : MonoBehaviour
    {
        [SerializeField] private T7_TutorialNPC npc;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip blipClip;

        public void Configure(T7_TutorialNPC targetNpc, AudioSource source, AudioClip blip)
        {
            if (isActiveAndEnabled && npc != null) npc.LineChanged -= HandleLineChanged;
            npc = targetNpc;
            audioSource = source;
            blipClip = blip;
            if (isActiveAndEnabled && npc != null) npc.LineChanged += HandleLineChanged;
        }

        private void OnEnable()
        {
            if (npc != null) npc.LineChanged += HandleLineChanged;
        }

        private void OnDisable()
        {
            if (npc != null) npc.LineChanged -= HandleLineChanged;
        }

        private void HandleLineChanged(int index, string line)
        {
            if (audioSource != null && blipClip != null)
                audioSource.PlayOneShot(blipClip);
        }
    }
}
