using UnityEngine;

namespace Tugas7
{
    public sealed class T7_CratePusher : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Rigidbody designatedCrate;
        [SerializeField] private float range = 6f;
        [SerializeField] private float impulse = 6f;

        public void Configure(Camera camera, Rigidbody crate)
        {
            playerCamera = camera;
            designatedCrate = crate;
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) || playerCamera == null || designatedCrate == null) return;
            if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                out RaycastHit hit, range) || hit.rigidbody != designatedCrate) return;
            designatedCrate.AddForceAtPosition(playerCamera.transform.forward * impulse,
                hit.point, ForceMode.Impulse);
        }
    }
}
