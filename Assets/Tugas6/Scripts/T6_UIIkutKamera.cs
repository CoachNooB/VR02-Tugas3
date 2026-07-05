using UnityEngine;

// =====================================================================
// T6_UIIkutKamera
// Bikin Canvas WORLD SPACE selalu mengikuti pandangan kamera (kayak HUD),
// TAPI tetap Render Mode World Space (bukan Overlay -> aman dari nilai 0).
// Tiap frame canvas dipindah ke depan kamera + menghadap kamera (billboard).
//
// Cara pakai:
// 1. Pasang di objek Canvas (Render Mode = World Space).
// 2. "Kamera" boleh dikosongkan -> otomatis pakai Camera.main.
// 3. Atur "Jarak" & "Geser" biar posisi HUD nyaman (gak nutup tengah layar).
// 4. Kalau teks kebalik saat Play, centang "Putar Balik".
// =====================================================================
public class T6_UIIkutKamera : MonoBehaviour
{
    [SerializeField] private Transform kamera;                        // auto Camera.main kalau kosong
    [SerializeField] private float jarak = 1.5f;                     // jarak di depan kamera
    [SerializeField] private Vector2 geser = new Vector2(0f, -0.35f); // geser kanan(x)/atas(y)
    [SerializeField] private bool putarBalik = false;                // centang kalau teks kebalik

    private void Awake()
    {
        if (kamera == null && Camera.main != null) kamera = Camera.main.transform;
    }

    // LateUpdate: dijalankan setelah kamera bergerak, biar HUD gak "telat" satu frame.
    private void LateUpdate()
    {
        if (kamera == null) return;

        transform.position = kamera.position + kamera.forward * jarak
                           + kamera.right * geser.x + kamera.up * geser.y;
        // Samakan rotasi dengan kamera -> canvas SEJAJAR layar (selalu persegi, bukan trapesium).
        transform.rotation = kamera.rotation;
        if (putarBalik) transform.Rotate(0f, 180f, 0f);
    }
}
