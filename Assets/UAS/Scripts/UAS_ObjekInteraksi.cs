using UnityEngine;

// =====================================================================
// UAS_ObjekInteraksi
// Objek yang bisa diinteraksi player (arahkan kamera + tekan E).
// SATU script dipakai ulang untuk macam-macam objek: tombol START,
// music box, tombol lampu, panel display, dll. Bedanya cukup diatur
// di Inspector (efek mana yang diisi).
//
// Cara pakai:
// 1. Pasang di objek interaktif. Pastikan objek punya Collider.
// 2. Isi efek yang diinginkan (semua opsional): suara, objek nyala, warna, kereta.
// =====================================================================
public class UAS_ObjekInteraksi : MonoBehaviour
{
    [Header("Feedback saat interaksi (semua opsional)")]
    [SerializeField] private AudioSource suara;           // dimainkan saat interaksi
    [SerializeField] private GameObject objekDinyalakan;  // mis. lampu/tirai -> di-toggle aktif/mati
    [SerializeField] private Renderer objekUbahWarna;     // ganti warna material saat interaksi
    [SerializeField] private Color warnaBaru = Color.green;
    [SerializeField] private UAS_DisplayAnimasi animasiDinyalakan; // mis. gate/tirai mulai bergerak saat ditekan

    [Header("Khusus tombol START (opsional)")]
    [SerializeField] private UAS_KeretaMover keretaStart;          // kalau diisi, interaksi memulai kereta
    [SerializeField] private UAS_FirstPersonController playerNaik; // kalau diisi, player nempel ke kursi kereta
    [SerializeField] private Transform kursiKereta;               // posisi duduk di kereta

    [Header("Pengaturan")]
    [SerializeField] private bool sekaliPakai = false;    // true = cuma bisa dipakai 1x (mis. tombol start)
    private bool sudahDipakai = false;

    // Dipanggil oleh UAS_RaycastInteractor saat player tekan E sambil mengarah ke objek ini.
    public void Interaksi()
    {
        if (sekaliPakai && sudahDipakai) return;
        sudahDipakai = true;

        if (suara != null) suara.Play();
        if (objekDinyalakan != null) objekDinyalakan.SetActive(!objekDinyalakan.activeSelf);
        if (objekUbahWarna != null) objekUbahWarna.material.color = warnaBaru;
        if (animasiDinyalakan != null) animasiDinyalakan.Nyalakan(); // transisi animasi akibat interaksi

        // kalau ini tombol START: naikin player ke kursi + jalankan kereta
        if (playerNaik != null && kursiKereta != null) playerNaik.NaikKereta(kursiKereta);
        if (keretaStart != null) keretaStart.MulaiJalan();
    }
}
