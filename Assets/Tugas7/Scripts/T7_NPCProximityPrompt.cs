using UnityEngine;

namespace Tugas7
{
    [RequireComponent(typeof(Collider))]
    public sealed class T7_NPCProximityPrompt : MonoBehaviour
    {
        [SerializeField] private T7_TutorialNPC npc;

        public void Configure(T7_TutorialNPC target) => npc = target;

        private void Reset()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void Update()
        {
            if (npc != null && npc.CanInteract && Input.GetKeyDown(KeyCode.E))
                npc.TryStartConversation();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (npc != null && other.CompareTag("Player"))
                npc.SetPlayerNearby(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (npc != null && other.CompareTag("Player"))
                npc.SetPlayerNearby(false);
        }
    }
}
