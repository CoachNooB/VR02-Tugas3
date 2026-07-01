using UnityEngine;

namespace Tugas7
{
    public sealed class T7_RaycastInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private T7_SpatialFeedbackUI ui;
        [SerializeField] private float range = 4f;
        [SerializeField] private LayerMask interactionMask = ~0;
        private T7_CourseInteractable target;

        public void Configure(Camera camera, T7_SpatialFeedbackUI spatialUI, float rayRange = 4f)
        {
            playerCamera = camera;
            ui = spatialUI;
            range = rayRange;
        }

        private void Awake()
        {
            if (playerCamera == null) playerCamera = Camera.main;
            if (ui == null) ui = FindAnyObjectByType<T7_SpatialFeedbackUI>();
        }

        private void Update()
        {
            T7_CourseInteractable next = null;
            if (playerCamera != null && Physics.Raycast(playerCamera.transform.position,
                playerCamera.transform.forward, out RaycastHit hit, range, interactionMask))
                next = hit.collider.GetComponentInParent<T7_CourseInteractable>();
            if (next != target)
            {
                target?.SetHighlighted(false);
                target = next;
                target?.SetHighlighted(true);
            }
            ui?.SetInteractionPrompt(target != null ? target.Prompt : string.Empty);
            if (target != null && Input.GetKeyDown(KeyCode.E))
                ui?.SetStatus(target.Interact());
        }
    }
}
