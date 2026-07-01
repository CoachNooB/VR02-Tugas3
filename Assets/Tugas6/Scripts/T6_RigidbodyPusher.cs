using UnityEngine;

// =====================================================================
// T6_RigidbodyPusher
// Klik KIRI mouse -> tembak ray dari kamera. Kalau kena objek ber-Rigidbody,
// objek didorong pakai AddForceAtPosition (ForceMode.Impulse = dorongan sekali).
// (Menggabungkan Raycast + Rigidbody, diajarkan di P10.)
//
// Cara pakai:
// 1. Pasang di Player. Drag "Kamera" = Main Camera.
// 2. Drag "Status UI" = Canvas World Space (T6_StatusUI).
// 3. Box target harus punya Rigidbody (Is Kinematic OFF, Use Gravity ON).
// 4. Layer Mask boleh "Everything" (default) -> tetap jalan.
// =====================================================================
public class T6_RigidbodyPusher : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Camera kamera;
    [SerializeField] private T6_StatusUI statusUI;

    [Header("Pengaturan")]
    [SerializeField] private float jarakRay = 5f;
    [SerializeField] private float gayaDorong = 5f;
    [SerializeField] private LayerMask layerMask = ~0; // ~0 = semua layer

    // Awake: isi reference otomatis kalau lupa di-drag di Inspector.
    private void Awake()
    {
        if (kamera == null) kamera = Camera.main;
        if (statusUI == null) statusUI = FindAnyObjectByType<T6_StatusUI>();
    }

    private void Update()
    {
        // hanya cek saat klik kiri
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = new Ray(kamera.transform.position, kamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, jarakRay, layerMask))
        {
            // ambil Rigidbody objek yang kena (setelah raycast, diizinkan)
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null)
            {
                // mainan awalnya diam (kinematic) -> bangunkan dulu biar bisa didorong
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
                // dorong ke arah depan kamera, dari titik yang kena ray
                rb.AddForceAtPosition(kamera.transform.forward * gayaDorong, hit.point, ForceMode.Impulse);
                if (statusUI != null) statusUI.SetInfo("Mendorong " + hit.collider.name);
            }
        }
    }
}
