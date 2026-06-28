using UnityEngine;

// =====================================================================
// UAS_DisplayAnimasi
// Animasi boneka/display TANPA Animator — cukup ubah transform di Update().
// Tinggal pilih mode di Inspector biar tiap display animasinya BEDA.
//
// Mode: 0 = putar, 1 = naik-turun, 2 = goyang, 3 = denyut (membesar-mengecil)
//
// Cara pakai:
// 1. Pasang di objek boneka/display.
// 2. Pilih "Mode" beda untuk tiap zona (syarat: min 3 animasi berbeda).
// =====================================================================
public class UAS_DisplayAnimasi : MonoBehaviour
{
    [Header("Mode (0=putar 1=naik-turun 2=goyang 3=denyut)")]
    [SerializeField] private int mode = 0;

    [Header("Pengaturan")]
    [SerializeField] private float kecepatan = 90f; // derajat/detik (putar & goyang)
    [SerializeField] private float jarak = 0.5f;    // tinggi naik-turun / besar denyut
    [SerializeField] private bool aktif = true;     // bisa dinyalakan trigger pakai Nyalakan()

    private Vector3 posisiAwal;
    private Vector3 skalaAwal;
    private float waktu;

    private void Start()
    {
        posisiAwal = transform.position;
        skalaAwal = transform.localScale;
    }

    private void Update()
    {
        if (!aktif) return;
        waktu += Time.deltaTime;

        if (mode == 0) // putar terus di sumbu Y (mis. komidi putar boneka)
        {
            transform.Rotate(0f, kecepatan * Time.deltaTime, 0f);
        }
        else if (mode == 1) // naik-turun melayang (mis. boneka ikan/astronot)
        {
            float y = Mathf.Sin(waktu * 2f) * jarak;
            transform.position = posisiAwal + new Vector3(0f, y, 0f);
        }
        else if (mode == 2) // goyang kiri-kanan (mis. boneka horror)
        {
            float sudut = Mathf.Sin(waktu * 3f) * kecepatan * 0.1f;
            transform.rotation = Quaternion.Euler(0f, 0f, sudut);
        }
        else if (mode == 3) // denyut membesar-mengecil
        {
            float s = 1f + Mathf.Sin(waktu * 3f) * 0.1f;
            transform.localScale = skalaAwal * s;
        }
    }

    // Dipanggil trigger biar animasi mulai pas kereta lewat (kalau awalnya "aktif" dimatikan).
    public void Nyalakan()
    {
        aktif = true;
    }
}
