using UnityEngine;

// =====================================================================
// UAS_KeretaMover
// Script penggerak kereta mini wahana boneka.
// Kereta jalan dari satu waypoint ke waypoint berikutnya (ikut jalur),
// jadi kereta TIDAK keluar dari jalur utama.
//
// Cara pakai (di Inspector):
// 1. Pasang script ini di GameObject kereta.
// 2. Buat beberapa GameObject kosong (Create Empty) sebagai titik jalur,
//    taruh berurutan di sepanjang track (Waypoint 1, 2, 3, ...).
// 3. Drag semua titik itu ke daftar "Waypoints" di Inspector, URUT.
// 4. Kereta mulai DIAM. Nyalakan lewat tombol Start / trigger "ride start"
//    yang memanggil fungsi MulaiJalan().
// =====================================================================
public class UAS_KeretaMover : MonoBehaviour
{
    [Header("Jalur (Waypoint)")]
    // Daftar titik jalur, diisi manual lewat Inspector (drag), harus urut.
    [SerializeField] private Transform[] waypoints;

    [Header("Pengaturan Gerak")]
    [SerializeField] private float kecepatan = 3f;       // kecepatan jalan (unit per detik)
    [SerializeField] private float jarakGanti = 0.2f;    // seberapa dekat baru pindah ke waypoint berikut

    [Header("Ride pacing (melambat sebelum berhenti)")]
    [SerializeField] private float jarakMelambat = 4f;   // mulai melambat sejauh ini dari titik berhenti
    [SerializeField] private float kecepatanMin = 0.8f;  // kecepatan paling pelan pas mau berhenti

    [Header("Belok (opsional)")]
    [SerializeField] private bool hadapArahJalan = true; // kereta menghadap arah jalannya
    [SerializeField] private float kecepatanBelok = 5f;  // kehalusan belok

    [Header("Berhenti di display (bonus stop point)")]
    // Centang index waypoint yang jadi tempat berhenti sebentar (mis. depan display).
    [SerializeField] private bool[] berhentiDiWaypoint;
    [SerializeField] private float durasiBerhenti = 2f; // lama berhenti (detik)

    [Header("Status (lihat saja saat Play)")]
    [SerializeField] private bool sedangJalan = false;   // mulai diam; dinyalakan tombol/trigger start
    [SerializeField] private bool sudahSelesai = false;  // true saat sampai waypoint terakhir

    // index waypoint yang sedang dituju
    private int indexTujuan = 0;
    private bool sedangBerhenti = false; // lagi berhenti di display?
    private float timerBerhenti = 0f;

    private void Update()
    {
        // Kalau belum disuruh jalan / belum ada waypoint -> diam saja.
        if (!sedangJalan) return;
        if (waypoints == null || waypoints.Length == 0) return;

        // Kalau lagi berhenti di display, hitung timer dulu (kereta diam).
        if (sedangBerhenti)
        {
            timerBerhenti += Time.deltaTime;
            if (timerBerhenti >= durasiBerhenti)
            {
                sedangBerhenti = false;
                timerBerhenti = 0f;
                LanjutWaypoint(); // lanjut ke waypoint berikutnya
            }
            return;
        }

        // Titik tujuan = waypoint yang sekarang.
        Transform tujuan = waypoints[indexTujuan];

        // Ride pacing: kalau waypoint ini titik berhenti, melambat saat mendekat.
        float kecepatanSekarang = kecepatan;
        if (berhentiDiWaypoint != null && indexTujuan < berhentiDiWaypoint.Length
            && berhentiDiWaypoint[indexTujuan])
        {
            float jarak = Vector3.Distance(transform.position, tujuan.position);
            if (jarak < jarakMelambat)
                kecepatanSekarang = Mathf.Lerp(kecepatanMin, kecepatan, jarak / jarakMelambat);
        }

        // Gerakkan kereta mendekati tujuan. MoveTowards membuat kereta lurus ke titik (tidak keluar jalur).
        transform.position = Vector3.MoveTowards(
            transform.position,
            tujuan.position,
            kecepatanSekarang * Time.deltaTime);

        // Hadapkan kereta ke arah jalan (opsional, biar kelihatan natural).
        if (hadapArahJalan)
        {
            Vector3 arah = tujuan.position - transform.position;
            arah.y = 0f; // jaga supaya kereta tidak nunduk/mendongak
            if (arah.sqrMagnitude > 0.001f)
            {
                Quaternion rotasiTujuan = Quaternion.LookRotation(arah);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    rotasiTujuan,
                    kecepatanBelok * Time.deltaTime);
            }
        }

        // Sudah sampai waypoint ini? Pindah ke waypoint berikutnya.
        if (Vector3.Distance(transform.position, tujuan.position) <= jarakGanti)
        {
            // Sampai waypoint: kalau ini titik berhenti, berhenti dulu sebentar.
            if (berhentiDiWaypoint != null && indexTujuan < berhentiDiWaypoint.Length
                && berhentiDiWaypoint[indexTujuan])
            {
                sedangBerhenti = true;
            }
            else
            {
                LanjutWaypoint();
            }
        }
    }

    // Pindah ke waypoint berikutnya. Kalau sudah 1 putaran penuh -> BERHENTI di titik akhir.
    private void LanjutWaypoint()
    {
        indexTujuan++;
        if (indexTujuan >= waypoints.Length)
        {
            // 1 putaran selesai -> kereta berhenti di waypoint terakhir (end state "Ride Complete").
            indexTujuan = waypoints.Length - 1;
            sedangJalan = false;
            sudahSelesai = true;
        }
    }

    // Dipanggil dari tombol Start / trigger "ride start". Mulai (atau ulangi) dari awal.
    public void MulaiJalan()
    {
        indexTujuan = 0;     // mulai dari waypoint pertama (biar bisa di-ride ulang)
        sudahSelesai = false;
        sedangJalan = true;
    }

    // Dipanggil dari trigger kalau ingin berhenti sementara (mis. di depan display).
    public void Berhenti()
    {
        sedangJalan = false;
    }

    // Bisa dibaca script lain (mis. UI) untuk tahu ride sudah selesai atau belum.
    public bool SudahSelesai()
    {
        return sudahSelesai;
    }
}
