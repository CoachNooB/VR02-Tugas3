using UnityEngine;

// =====================================================================
// T6_TriggerZone
// Area trigger: saat Player MASUK & KELUAR area, ubah status UI.
// Bisa juga (BONUS) membuka kunci satu objek saat player masuk.
//
// Cara pakai:
// 1. Buat Cube, aktifkan Box Collider, CENTANG "Is Trigger".
//    Mesh Renderer boleh dimatikan / dibuat transparan.
// 2. Pasang script ini. Drag "Status UI" = Canvas World Space (T6_StatusUI).
// 3. Isi teks status masuk & keluar.
// 4. (Bonus) Drag "Toy Terkunci" = objek T6_Interactable yang mau dibuka saat masuk.
// 5. Pastikan Player ber-Tag "Player" dan punya Rigidbody (kinematic pun cukup).
// =====================================================================
public class T6_TriggerZone : MonoBehaviour
{
    [Header("Deteksi")]
    [SerializeField] private string tagPemicu = "Player";

    [Header("Status UI")]
    [SerializeField] private T6_StatusUI statusUI;
    [SerializeField] private string statusMasuk = "Di dalam area pajangan";
    [SerializeField] private string statusKeluar = "Di luar area";

    [Header("Feedback (opsional)")]
    [SerializeField] private GameObject objekDinyalakan; // mis. lampu area -> nyala saat masuk

    [Header("Bonus: buka kunci objek saat masuk (opsional)")]
    [SerializeField] private T6_Interactable toyTerkunci;

    // Awake: isi reference otomatis kalau lupa di-drag di Inspector.
    private void Awake()
    {
        if (statusUI == null) statusUI = FindAnyObjectByType<T6_StatusUI>();
        if (toyTerkunci == null)
        {
            GameObject g = GameObject.Find("T6_Toy4_Bonus"); // objek bonus yang dibuka
            if (g != null) toyTerkunci = g.GetComponent<T6_Interactable>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagPemicu)) return;

        if (statusUI != null) statusUI.SetStatus(statusMasuk);
        if (objekDinyalakan != null) objekDinyalakan.SetActive(true);
        if (toyTerkunci != null) toyTerkunci.Buka(); // bonus: objek jadi bisa di-E
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(tagPemicu)) return;

        if (statusUI != null) statusUI.SetStatus(statusKeluar);
        if (objekDinyalakan != null) objekDinyalakan.SetActive(false);
    }
}
