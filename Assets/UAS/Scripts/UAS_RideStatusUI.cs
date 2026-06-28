using UnityEngine;
using TMPro;

// =====================================================================
// UAS_RideStatusUI
// UI utama wahana di WORLD SPACE CANVAS (wajib World Space, bukan Overlay).
// Tampilkan status ride (Ready/Moving/Ride Complete) + checklist zona.
//
// Cara pakai:
// 1. Buat Canvas (Render Mode = World Space), taruh di dunia (mis. di atas boarding).
// 2. Di dalamnya buat 2 Text (TextMeshPro): teks status & teks checklist.
// 3. Pasang script ini di Canvas, drag 2 teks itu, isi "Nama Zona" urut.
// 4. Trigger zona memanggil SetStatus(...) & TandaiZona(...).
// =====================================================================
public class UAS_RideStatusUI : MonoBehaviour
{
    [Header("Teks UI (World Space)")]
    [SerializeField] private TextMeshProUGUI teksStatus;    // "Status: Ready"
    [SerializeField] private TextMeshProUGUI teksChecklist; // daftar zona

    [Header("Daftar zona (isi manual, urut)")]
    [SerializeField] private string[] namaZona; // mis. Hutan, Laut, Horror, Angkasa

    private bool[] sudahDilewati;

    private void Start()
    {
        sudahDilewati = new bool[namaZona.Length];
        SetStatus("Ready");
        GambarChecklist();
    }

    // Ganti teks status ride. Dipanggil trigger (mis. "Moving", "Ride Complete").
    public void SetStatus(string status)
    {
        if (teksStatus != null) teksStatus.text = "Status: " + status;
    }

    // Tandai satu zona sudah dilewati. Dipanggil display trigger.
    public void TandaiZona(string nama)
    {
        for (int i = 0; i < namaZona.Length; i++)
        {
            if (namaZona[i] == nama) sudahDilewati[i] = true;
        }
        GambarChecklist();
    }

    // Susun ulang teks checklist dari data.
    private void GambarChecklist()
    {
        if (teksChecklist == null) return;
        string isi = "Zona:\n";
        for (int i = 0; i < namaZona.Length; i++)
        {
            string tanda = sudahDilewati[i] ? "[v] " : "[ ] ";
            isi = isi + tanda + namaZona[i] + "\n";
        }
        teksChecklist.text = isi;
    }
}
