using UnityEngine;

namespace Tugas7
{
    public sealed class T7_NPCHeadHitInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField, Min(0.1f)] private float range = 3f;

        public void Configure(Camera camera, float hitRange = 3f)
        {
            playerCamera = camera;
            range = Mathf.Max(0.1f, hitRange);
        }

        public bool TryHit(Ray ray)
        {
            if (!Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
                return false;
            T7_TutorialNPC npc = hit.collider.GetComponentInParent<T7_TutorialNPC>();
            return npc != null && npc.TryPlayHeadHit();
        }

        private void Update()
        {
            if (playerCamera == null || !Input.GetMouseButtonDown(0))
                return;
            TryHit(new Ray(playerCamera.transform.position, playerCamera.transform.forward));
        }
    }
}
