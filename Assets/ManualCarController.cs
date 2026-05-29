using UnityEngine;

public class CarControllerManual : MonoBehaviour
{
    [Header("Penggerak")]
    public float moveSpeed = 8f;      // kecepatan maju/mundur
    public float turnSpeed = 100f;    // kecepatan belok (derajat per detik)

    [Header("Posisi Tetap")]
    public float fixedY = 0.65f;      // tinggi mobil (disesuaikan dengan permukaan)

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        // Pengaturan fisika
        rb.mass = 1f;
        rb.linearDamping = 1f;          // gesekan saat bergerak
        rb.angularDamping = 2f;   // gesekan rotasi agar lebih stabil
        rb.useGravity = true;

        // Kunci rotasi sumbu X dan Z (agar mobil tidak miring ke samping)
        // Sumbu Y bebas untuk belok
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Kursor tidak perlu dikunci karena tidak pakai mouse untuk belok
        // (biarkan bebas, tapi untuk kenyamanan, tetap kunci? terserah)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pastikan posisi Y sesuai
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    void Update()
    {
        // ---- INPUT BELOK (A / D) ----
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;  // belok kiri
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;   // belok kanan

        // Rotasi mobil pada sumbu Y (belok)
        float turn = turnInput * turnSpeed * Time.deltaTime;
        transform.Rotate(0, turn, 0);

        // ---- INPUT MAJU / MUNDUR (W / S) ----
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.W)) moveInput = 1f;   // maju
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;  // mundur

        // Gerak maju/mundur sesuai arah hadap mobil
        Vector3 moveDirection = transform.forward * moveInput;
        moveDirection.Normalize();

        Vector3 desiredVelocity = moveDirection * moveSpeed;
        desiredVelocity.y = rb.linearVelocity.y; // biarkan gravitasi bekerja (tapi akan dipertahankan di FixedUpdate)

        rb.linearVelocity = desiredVelocity;
    }

    void FixedUpdate()
    {
        // Pertahankan ketinggian mobil (tidak jatuh atau melayang)
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.y - fixedY) > 0.02f)
        {
            pos.y = fixedY;
            transform.position = pos;
            // Hentikan kecepatan vertikal
            Vector3 vel = rb.linearVelocity;
            vel.y = 0;
            rb.linearVelocity = vel;
        }
    }
}