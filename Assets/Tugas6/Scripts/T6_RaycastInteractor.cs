using UnityEngine;

// =====================================================================
// T6_RaycastInteractor
// Tembak garis (ray) dari kamera ke depan SETIAP frame.
//  - Kalau kena objek T6_Interactable  -> objek di-HIGHLIGHT + tampil namanya di UI.
//  - Kalau player tekan E sambil melihat -> jalankan Interaksi() objek itu.
// (Teknik Physics.Raycast + GetComponent setelah raycast diajarkan di P10.)
//
// Cara pakai:
// 1. Pasang di Player. Drag "Kamera" = Main Camera.
// 2. Drag "Status UI" = objek Canvas World Space (T6_StatusUI).
// 3. Layer Mask boleh dibiarkan "Everything" (default) -> tetap jalan.
// =====================================================================
public class T6_RaycastInteractor : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Camera kamera;
    [SerializeField] private T6_StatusUI statusUI;

    [Header("Pengaturan")]
    [SerializeField] private float jarakRay = 3f;             // jangkauan interaksi (unit)
    [SerializeField] private LayerMask layerMask = ~0;        // ~0 = semua layer
    [SerializeField] private KeyCode tombol = KeyCode.E;

    private T6_Interactable targetSekarang; // objek yang sedang dilihat

    // Awake: isi reference otomatis kalau lupa di-drag di Inspector.
    private void Awake()
    {
        if (kamera == null) kamera = Camera.main;
        if (statusUI == null) statusUI = FindAnyObjectByType<T6_StatusUI>();
    }

    private void Update()
    {
        // 1) tembak ray dari kamera ke depan
        Ray ray = new Ray(kamera.transform.position, kamera.transform.forward);
        RaycastHit hit;
        T6_Interactable targetBaru = null;

        if (Physics.Raycast(ray, out hit, jarakRay, layerMask))
        {
            // GetComponent SETELAH raycast (diizinkan materi P10)
            targetBaru = hit.collider.GetComponent<T6_Interactable>();
        }

        // 2) update highlight kalau target berganti
        if (targetBaru != targetSekarang)
        {
            if (targetSekarang != null) targetSekarang.Sorot(false); // matikan highlight lama
            if (targetBaru != null) targetBaru.Sorot(true);          // nyalakan highlight baru
            targetSekarang = targetBaru;

            // tampilkan info di UI
            if (statusUI != null)
            {
                if (targetBaru != null) statusUI.SetInfo("Melihat objek — tekan E");
                else statusUI.SetInfo("Arahkan pandangan ke objek");
            }
        }

        // 3) tekan E untuk interaksi
        if (Input.GetKeyDown(tombol) && targetSekarang != null)
        {
            string pesan = targetSekarang.Interaksi();
            if (statusUI != null) statusUI.SetInfo(pesan);
        }
    }
}
