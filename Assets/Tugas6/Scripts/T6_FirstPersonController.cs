using UnityEngine;

// =====================================================================
// T6_FirstPersonController
// Controller first-person PAKAI PHYSICS (Rigidbody dinamis) supaya:
//  - player nabrak dinding/meja (tidak bisa menembus),
//  - kena gravitasi (jatuh & berdiri di lantai),
//  - bisa LOMPAT (Space) & mendorong mainan Rigidbody saat nabrak.
// Gerak WASD + lihat mouse. (Materi P10: Rigidbody, Collider, physics.)
//
// Cara pakai:
// 1. Pasang di "Player" (Tag "Player"). Rigidbody: Is Kinematic OFF,
//    Use Gravity ON, Freeze Rotation X/Y/Z (biar tidak jatuh terguling).
// 2. Player wajib punya Collider (Capsule).
// 3. Drag "Kamera Player" = Main Camera (child), "Rb" = Rigidbody player.
// =====================================================================
public class T6_FirstPersonController : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Transform kameraPlayer; // kamera (child Player)
    [SerializeField] private Rigidbody rb;           // Rigidbody player (dinamis)

    [Header("Gerak")]
    [SerializeField] private float kecepatanJalan = 4f;
    [SerializeField] private float sensitivitasMouse = 2f;

    [Header("Lompat")]
    [SerializeField] private float gayaLompat = 5f;
    [SerializeField] private float jarakCekTanah = 1.2f; // panjang ray ke bawah cek lantai
    [SerializeField] private LayerMask layerTanah = ~0;  // ~0 = semua layer

    private float sudutPitch = 0f; // sudut kamera atas-bawah
    private CapsuleCollider kapsulPlayer;

    // Awake: isi reference otomatis kalau lupa di-drag (Camera.main & GetComponent = contoh slide P10).
    private void Awake()
    {
        if (kameraPlayer == null && Camera.main != null) kameraPlayer = Camera.main.transform;
        if (rb == null) rb = GetComponent<Rigidbody>();
        kapsulPlayer = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // kunci kursor di tengah layar
        if (rb != null)
        {
            rb.isKinematic = false;                 // dinamis: ikut physics
            rb.freezeRotation = true;               // jangan terguling
        }
    }

    private void Update()
    {
        LihatMouse();
        if (Input.GetKeyDown(KeyCode.Space) && DiTanah()) Lompat();
    }

    // Gerak fisik dilakukan di FixedUpdate (waktu physics).
    private void FixedUpdate()
    {
        Jalan();
    }

    // Jalan WASD: set kecepatan horizontal, tapi kecepatan vertikal (gravitasi/lompat) dibiarkan.
    private void Jalan()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 arah = (transform.right * h + transform.forward * v).normalized;

        Vector3 kecepatan = arah * kecepatanJalan;
        kecepatan.y = rb.linearVelocity.y;     // jaga gerak naik/turun dari physics
        rb.linearVelocity = kecepatan;
    }

    // Putar pandangan pakai mouse.
    private void LihatMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivitasMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivitasMouse;

        transform.Rotate(0f, mouseX, 0f); // badan player muter kiri-kanan

        sudutPitch -= mouseY;
        sudutPitch = Mathf.Clamp(sudutPitch, -80f, 80f); // batasi biar gak kebalik
        kameraPlayer.localRotation = Quaternion.Euler(sudutPitch, 0f, 0f);
    }

    // Lompat: dorong ke atas dengan mengganti kecepatan-y.
    private void Lompat()
    {
        Vector3 v = rb.linearVelocity;
        v.y = gayaLompat;
        rb.linearVelocity = v;
    }

    // Cek apakah player menyentuh tanah (tembak ray pendek ke bawah).
    private bool DiTanah()
    {
        if (kapsulPlayer != null)
            return Tugas7.T7_GroundProbe.IsGrounded(transform, kapsulPlayer, jarakCekTanah, layerTanah);
        return Physics.Raycast(transform.position, Vector3.down, jarakCekTanah, layerTanah,
            QueryTriggerInteraction.Ignore);
    }
}
