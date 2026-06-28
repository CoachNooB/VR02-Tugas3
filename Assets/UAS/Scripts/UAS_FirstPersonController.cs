using UnityEngine;

// =====================================================================
// UAS_FirstPersonController
// Controller first-person sederhana: jalan WASD + lihat pakai mouse.
// Gerak pakai transform.Translate (Rigidbody di-set Kinematic biar gak
// digeret physics). Sederhana & pasti jalan.
//
// Cara pakai:
// 1. Pasang di "Player" (Tag "Player"), Rigidbody = Is Kinematic ON, gravity OFF.
// 2. Drag Kamera Player = Main Camera (child), Titik Awal = UAS_TitikAwal.
// =====================================================================
public class UAS_FirstPersonController : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField] private Transform kameraPlayer; // kamera (child Player)
    [SerializeField] private Rigidbody rb;           // Rigidbody player (di-set kinematic)
    [SerializeField] private Transform titikAwal;    // posisi balik ke awal setelah ride

    [Header("Gerak")]
    [SerializeField] private float kecepatanJalan = 4f;
    [SerializeField] private float sensitivitasMouse = 2f;

    [Header("Status")]
    [SerializeField] private bool bisaJalan = true;  // dimatikan saat naik kereta

    private float sudutPitch = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        // pastikan player tidak digeret physics
        if (rb != null) rb.isKinematic = true;
    }

    private void Update()
    {
        LihatMouse();
        if (bisaJalan) Jalan();
    }

    // Jalan WASD pakai transform (langsung, gak lewat physics).
    private void Jalan()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S
        Vector3 arah = transform.right * h + transform.forward * v;
        transform.Translate(arah * kecepatanJalan * Time.deltaTime, Space.World);
    }

    // Putar pandangan pakai mouse.
    private void LihatMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivitasMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivitasMouse;

        transform.Rotate(0f, mouseX, 0f);

        sudutPitch -= mouseY;
        sudutPitch = Mathf.Clamp(sudutPitch, -80f, 80f);
        kameraPlayer.localRotation = Quaternion.Euler(sudutPitch, 0f, 0f);
    }

    // Dipanggil tombol START: player nempel ke kursi kereta.
    public void NaikKereta(Transform kursi)
    {
        transform.SetParent(kursi);
        transform.localPosition = Vector3.zero;
        bisaJalan = false;
    }

    // Dipanggil finish: player turun & balik ke titik awal.
    public void TurunKereta()
    {
        transform.SetParent(null);
        if (titikAwal != null)
        {
            transform.position = titikAwal.position;
            transform.rotation = titikAwal.rotation;
        }
        bisaJalan = true;
    }
}
