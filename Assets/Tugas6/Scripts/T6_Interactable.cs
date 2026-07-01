using UnityEngine;

// =====================================================================
// T6_Interactable
// Dipasang di tiap "mainan" (boneka/hewan/ikan) yang bisa:
//  - di-HIGHLIGHT saat dilihat raycast: MENYALA lembut (emission glow) +
//    sedikit MEMBESAR (kesan mewah, bukan warna solid), dan
//  - di-INTERACT saat player tekan E (jalankan feedback).
//
// Cara pakai:
// 1. Pasang di objek mainan. Pastikan punya Collider (buat kena raycast).
// 2. "Renderer Objek" boleh dikosongkan -> otomatis cari Renderer di objek
//    ini atau di child-nya (buat model yang mesh-nya di child).
// 3. Atur warna glow, nama, & feedback interaksi.
// 4. BONUS: centang "Terkunci" -> baru bisa di-E setelah trigger memanggil Buka().
// =====================================================================
public class T6_Interactable : MonoBehaviour
{
    [Header("Identitas")]
    [SerializeField] private string namaObjek = "Mainan";

    [Header("Highlight (glow + membesar saat dilihat)")]
    [SerializeField] private Renderer rendererObjek;             // renderer objek (auto kalau kosong)
    [SerializeField] private Color warnaGlow = new Color(1f, 0.95f, 0.85f); // hangat lembut
    [SerializeField] private float intensitasGlow = 0.3f;      // rendah = glow halus "kena cahaya", bukan putih blok
    [SerializeField] private float faktorMembesar = 1.05f;     // 1.05 = membesar 5% (pop halus)

    [Header("Feedback saat interaksi E (semua opsional)")]
    [SerializeField] private AudioSource suara;
    [SerializeField] private GameObject objekToggle;            // mis. lampu -> nyala/mati
    [SerializeField] private bool ubahWarnaSaatInteraksi = false;
    [SerializeField] private Color warnaHasilInteraksi = Color.green;
    [SerializeField] private string pesanInteraksi = "Mainan berinteraksi!";

    [Header("Bonus: objek terkunci")]
    [SerializeField] private bool terkunci = false;

    private Vector3 skalaAwal;
    private bool disorot;
    private Rigidbody rb; // buat "membangunkan" fisika saat mainan kena/diklik

    private void Start()
    {
        // cari renderer otomatis: di objek ini, atau di child (buat model boneka/hewan)
        if (rendererObjek == null) rendererObjek = GetComponent<Renderer>();
        if (rendererObjek == null) rendererObjek = GetComponentInChildren<Renderer>();

        if (objekToggle == null)
        {
            Transform t = transform.Find("T6_Toggle");
            if (t != null) objekToggle = t.gameObject;
        }

        rb = GetComponent<Rigidbody>();
        skalaAwal = transform.localScale;
    }

    // Mainan awalnya DIAM (Rigidbody kinematic). Saat player nabrak/lompatin -> hidupkan fisika
    // supaya mainan bisa jatuh/berantakan.
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player")) AktifkanFisika();
    }

    // Ubah mainan jadi ikut physics (dipanggil saat ditabrak player atau diklik/didorong).
    public void AktifkanFisika()
    {
        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    // Dipanggil interactor tiap frame: true saat dilihat, false saat tidak.
    public void Sorot(bool aktif)
    {
        if (disorot == aktif) return; // biar gak set terus-terusan
        disorot = aktif;

        if (rendererObjek != null)
        {
            Material m = rendererObjek.material;
            if (aktif)
            {
                m.EnableKeyword("_EMISSION");                  // nyalakan emission
                m.SetColor("_EmissionColor", warnaGlow * intensitasGlow);
            }
            else
            {
                m.SetColor("_EmissionColor", Color.black);     // matikan glow
            }
        }

        // sedikit membesar saat dilihat (efek "pop")
        transform.localScale = aktif ? skalaAwal * faktorMembesar : skalaAwal;
    }

    // Dipanggil interactor saat player tekan E sambil melihat objek ini.
    public string Interaksi()
    {
        if (terkunci) return namaObjek + " terkunci";

        if (suara != null) suara.Play();
        if (objekToggle != null) objekToggle.SetActive(!objekToggle.activeSelf);
        if (ubahWarnaSaatInteraksi && rendererObjek != null)
            rendererObjek.material.color = warnaHasilInteraksi;

        return pesanInteraksi;
    }

    // Dipanggil trigger zone (bonus): buka kunci objek.
    public void Buka()
    {
        terkunci = false;
    }
}
