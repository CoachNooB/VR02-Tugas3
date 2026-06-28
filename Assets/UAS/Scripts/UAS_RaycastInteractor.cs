using UnityEngine;

// =====================================================================
// UAS_RaycastInteractor
// Tembak garis (ray) dari kamera ke depan. Kalau kena objek yang punya
// script UAS_ObjekInteraksi dan player tekan E, jalankan interaksinya.
// (Teknik raycast + GetComponent ini diajarkan di materi P10.)
//
// Cara pakai:
// 1. Pasang di Player (atau kamera). Drag Main Camera ke "Kamera".
// =====================================================================
public class UAS_RaycastInteractor : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Camera kamera;

    [Header("Pengaturan")]
    [SerializeField] private float jarakRay = 3f;        // jangkauan interaksi (unit)
    [SerializeField] private KeyCode tombol = KeyCode.E;

    private void Update()
    {
        // hanya cek saat tombol E ditekan
        if (!Input.GetKeyDown(tombol)) return;

        // ray dari posisi kamera, lurus ke arah depan kamera
        Ray ray = new Ray(kamera.transform.position, kamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, jarakRay))
        {
            // ambil script interaksi dari objek yang kena (kalau ada)
            UAS_ObjekInteraksi objek = hit.collider.GetComponent<UAS_ObjekInteraksi>();
            if (objek != null)
            {
                objek.Interaksi();
            }
        }
    }
}
