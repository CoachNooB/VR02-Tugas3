using UnityEngine;

// =====================================================================
// UAS_SequenceDisplay
// Sequence animasi 3 tahap untuk satu display (bonus +5).
// Saat dipicu (Mulai()), berurutan: lampu nyala -> boneka gerak -> musik.
// Tanpa Animator/coroutine — cuma timer float di Update().
//
// Cara pakai:
// 1. Pasang di salah satu display.
// 2. Isi: Lampu (GameObject), Animasi Boneka (UAS_DisplayAnimasi), Musik (AudioSource).
// 3. Picu lewat trigger display (panggil Mulai()) atau centang "Mulai Otomatis".
// =====================================================================
public class UAS_SequenceDisplay : MonoBehaviour
{
    [Header("Tahap 1: lampu nyala")]
    [SerializeField] private GameObject lampu;
    [SerializeField] private float waktuTahap1 = 0.5f;

    [Header("Tahap 2: boneka gerak")]
    [SerializeField] private UAS_DisplayAnimasi animasiBoneka;
    [SerializeField] private float waktuTahap2 = 1.5f;

    [Header("Tahap 3: musik")]
    [SerializeField] private AudioSource musik;
    [SerializeField] private float waktuTahap3 = 2.5f;

    [Header("Pengaturan")]
    [SerializeField] private bool mulaiOtomatis = false; // mulai sendiri saat Start

    private bool sedangJalan = false;
    private float timer = 0f;
    private int tahap = 0; // 0 = belum, 1/2/3 = tahap yang sudah dijalankan

    private void Start()
    {
        if (mulaiOtomatis) Mulai();
    }

    private void Update()
    {
        if (!sedangJalan) return;
        timer += Time.deltaTime;

        // Tahap 1: lampu nyala
        if (tahap < 1 && timer >= waktuTahap1)
        {
            tahap = 1;
            if (lampu != null) lampu.SetActive(true);
        }
        // Tahap 2: boneka mulai bergerak
        else if (tahap < 2 && timer >= waktuTahap2)
        {
            tahap = 2;
            if (animasiBoneka != null) animasiBoneka.Nyalakan();
        }
        // Tahap 3: musik nyala, sequence selesai
        else if (tahap < 3 && timer >= waktuTahap3)
        {
            tahap = 3;
            if (musik != null) musik.Play();
            sedangJalan = false;
        }
    }

    // Dipanggil trigger/tombol untuk memulai sequence dari awal.
    public void Mulai()
    {
        sedangJalan = true;
        timer = 0f;
        tahap = 0;
    }
}
