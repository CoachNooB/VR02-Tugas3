using UnityEngine;

// =====================================================================
// UAS_TriggerZone
// Area trigger: saat objek ber-tag tertentu (Player/Kereta) MASUK area,
// kasih efek (suara/lampu/objek nyala) + bisa atur kereta (start/berhenti)
// + update status UI. Dipakai untuk boarding, ride start, display, finish.
//
// Cara pakai:
// 1. Buat objek kosong / cube, kasih Collider dengan "Is Trigger" DICENTANG.
// 2. Pasang script ini, atur Tag pemicu & efek yang diinginkan di Inspector.
// 3. Pastikan Player/Kereta punya Tag yang cocok + (salah satunya) Rigidbody
//    biar OnTriggerEnter kebaca.
// =====================================================================
public class UAS_TriggerZone : MonoBehaviour
{
    [Header("Deteksi")]
    [SerializeField] private string tagPemicu = "Player"; // "Player" atau "Kereta"

    [Header("Feedback (opsional)")]
    [SerializeField] private AudioSource suara;
    [SerializeField] private GameObject objekDinyalakan;  // lampu/musik/boneka di-set aktif
    [SerializeField] private Light lampuZona;
    [SerializeField] private Color warnaLampu = Color.white;
    [SerializeField] private UAS_DisplayAnimasi animasiDisplay; // nyalain animasi boneka

    [Header("Aksi kereta (opsional)")]
    [SerializeField] private UAS_KeretaMover kereta;
    [SerializeField] private bool mulaikanKereta = false;
    [SerializeField] private bool hentikanKereta = false;

    [Header("Turunin player (opsional, buat finish)")]
    [SerializeField] private UAS_FirstPersonController playerTurun; // kalau diisi, player turun & balik ke awal

    [Header("Status UI (opsional)")]
    [SerializeField] private UAS_RideStatusUI statusUI;
    [SerializeField] private string statusBaru = "";  // mis. "Moving", "Ride Complete"
    [SerializeField] private string namaZona = "";    // utk checklist, mis. "Hutan"

    private bool sudahKena = false; // biar efek cuma jalan sekali

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagPemicu)) return;
        if (sudahKena) return;
        sudahKena = true;

        // feedback audio/visual
        if (suara != null) suara.Play();
        if (objekDinyalakan != null) objekDinyalakan.SetActive(true);
        if (lampuZona != null) lampuZona.color = warnaLampu;
        if (animasiDisplay != null) animasiDisplay.Nyalakan();

        // atur kereta
        if (kereta != null && mulaikanKereta) kereta.MulaiJalan();
        if (kereta != null && hentikanKereta) kereta.Berhenti();

        // update UI status
        if (statusUI != null)
        {
            if (statusBaru != "") statusUI.SetStatus(statusBaru);
            if (namaZona != "") statusUI.TandaiZona(namaZona);
        }
    }
}
