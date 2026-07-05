using UnityEngine;
using TMPro;

// =====================================================================
// T6_StatusUI
// UI utama di WORLD SPACE CANVAS (WAJIB World Space, bukan Overlay!).
// Menampilkan 2 baris teks:
//  - teksStatus : status trigger zone (mis. "Di dalam area pajangan")
//  - teksInfo   : objek yang sedang dilihat / hasil interaksi
//
// Cara pakai:
// 1. Buat Canvas (Render Mode = World Space), taruh di dunia (mis. nempel dinding).
// 2. Di dalamnya buat 2 Text (TextMeshPro): teks status & teks info.
// 3. Pasang script ini di Canvas, drag 2 teks itu ke field.
// 4. Script lain (trigger/interactor/pusher) memanggil SetStatus/SetInfo.
// =====================================================================
public class T6_StatusUI : MonoBehaviour
{
    [Header("Teks UI (World Space)")]
    [SerializeField] private TextMeshProUGUI teksStatus; // status trigger zone
    [SerializeField] private TextMeshProUGUI teksInfo;   // objek dilihat / hasil interaksi

    // Awake: cari teks child otomatis kalau lupa di-drag di Inspector.
    private void Awake()
    {
        if (teksStatus == null)
        {
            Transform t = transform.Find("T6_TeksStatus");
            if (t != null) teksStatus = t.GetComponent<TextMeshProUGUI>();
        }
        if (teksInfo == null)
        {
            Transform t = transform.Find("T6_TeksInfo");
            if (t != null) teksInfo = t.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        SetStatus("Di luar area");
        SetInfo("Arahkan pandangan ke objek");
    }

    // Ganti baris status (dipanggil trigger zone saat masuk/keluar).
    public void SetStatus(string status)
    {
        if (teksStatus != null) teksStatus.text = "Status: " + status;
    }

    // Ganti baris info (dipanggil interactor/pusher).
    public void SetInfo(string info)
    {
        if (teksInfo != null) teksInfo.text = info;
    }
}
