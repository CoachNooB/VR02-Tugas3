using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class TrainControllerManual : MonoBehaviour
{
    [Header("Track")]
    public List<Transform> waypoints;          // Titik jalur (harus diisi)
    public bool loop = true;

    [Header("Driving")]
    public float maxSpeed = 20f;               // Kecepatan maksimum (m/s)
    public float acceleration = 10f;            // Akselerasi maju
    public float brakeDeceleration = 20f;       // Perlambatan saat rem
    public float drag = 2f;                    // Gesekan saat tidak menekan gas

    [Header("Train Settings")]
    public float smoothing = 5f;                // Kehalusan rotasi
    public float tiltAngle = 2f;                // Kemiringan saat belok

    private Rigidbody rb;
    private float currentSpeed = 0f;            // Kecepatan linear sepanjang track (positif = maju)

    // Variabel untuk melacak posisi di antara waypoint
    private int currentSegment = 0;             // Indeks waypoint awal segmen
    private float segmentProgress = 0f;         // 0..1 antara waypoint[currentSegment] dan waypoint[currentSegment+1]

    // Input
    private float throttleInput = 0f;           // -1 (mundur) hingga 1 (maju)
    private bool brakePressed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (waypoints == null || waypoints.Count < 2)
        {
            Debug.LogError("TrainControllerManual: waypoints not set or less than 2!");
            enabled = false;
            return;
        }

        // Posisi awal di waypoint pertama
        transform.position = waypoints[0].position;
        // Arah ke waypoint berikutnya
        Vector3 dir = (waypoints[1].position - waypoints[0].position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
        currentSegment = 0;
        segmentProgress = 0f;
    }

    private void Update()
    {
        // Baca input WASD
        // W = maju, S = mundur, Spasi = rem (opsional, kita gunakan S untuk mundur dan rem terpisah? 
        // Kita akan gunakan axis vertical: W = 1, S = -1
        throttleInput = Input.GetAxis("Vertical");   // -1..1
        brakePressed = Input.GetKey(KeyCode.Space); // rem

        // Juga bisa pakai tombol panah atas/bawah
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

        // Hitung percepatan berdasarkan input
        float accel = 0f;
        if (brakePressed)
        {
            // Rem: perlambatan cepat
            accel = -Mathf.Sign(currentSpeed) * brakeDeceleration;
            if (Mathf.Abs(currentSpeed) < 0.1f) currentSpeed = 0f;
        }
        else
        {
            // Gas
            if (Mathf.Abs(throttleInput) > 0.01f)
            {
                // Maju atau mundur
                float targetSpeed = throttleInput * maxSpeed;
                // Akselerasi menuju target
                float diff = targetSpeed - currentSpeed;
                accel = Mathf.Sign(diff) * acceleration;
                // Batasi akselerasi agar tidak overshoot
                if (Mathf.Abs(diff) < acceleration * Time.fixedDeltaTime)
                {
                    currentSpeed = targetSpeed;
                    accel = 0f;
                }
            }
            else
            {
                // Tanpa gas: drag
                accel = -Mathf.Sign(currentSpeed) * drag;
                if (Mathf.Abs(currentSpeed) < drag * Time.fixedDeltaTime)
                    currentSpeed = 0f;
            }
        }

        // Terapkan percepatan ke kecepatan
        currentSpeed += accel * Time.fixedDeltaTime;
        // Batasi kecepatan maksimum (nilai absolut)
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);

        // Jika kecepatan hampir 0, set ke 0 untuk mencegah osilasi
        if (Mathf.Abs(currentSpeed) < 0.001f)
            currentSpeed = 0f;

        // Gerakkan sepanjang track berdasarkan kecepatan
        float distanceStep = currentSpeed * Time.fixedDeltaTime;

        // Update posisi sepanjang segmen
        UpdatePositionOnTrack(distanceStep);

        // Rotasi mengikuti arah track
        UpdateRotation();
    }

    private void UpdatePositionOnTrack(float distance)
    {
        if (waypoints.Count < 2) return;

        // Cari panjang segmen saat ini
        int nextIndex = (currentSegment + 1) % waypoints.Count;
        float segLength = Vector3.Distance(waypoints[currentSegment].position, waypoints[nextIndex].position);

        // Ubah jarak menjadi perubahan progress
        float deltaProgress = distance / segLength;
        segmentProgress += deltaProgress;

        // Periksa apakah progress melewati batas segmen
        if (segmentProgress >= 1f)
        {
            // Pindah ke segmen berikutnya
            currentSegment = nextIndex;
            segmentProgress -= 1f;
            if (!loop && currentSegment >= waypoints.Count - 1)
            {
                // Di ujung, stop
                currentSpeed = 0f;
                segmentProgress = 0f;
                // Tetap di posisi terakhir
                transform.position = waypoints[waypoints.Count - 1].position;
                return;
            }
            // Jika loop, currentSegment akan kembali ke 0 setelah melewati akhir
            if (loop && currentSegment >= waypoints.Count - 1)
            {
                // Saat mencapai akhir, kita perlu wrap ke awal dengan cara khusus
                // Karena kita pakai modulo, kita set currentSegment ke 0 dan progress tambahan?
                // Lebih mudah: kita gunakan loop dengan cara menghitung total panjang track dan posisi absolut.
                // Tapi untuk sederhana, kita tangani kasus wrap di sini.
                // Untuk loop, kita akan memindahkan ke segmen 0 dan mengurangi progress jika melewati.
                // Namun karena kita menggunakan modulo pada indeks, kita bisa melakukan wrap.
                // Cara: jika kita melewati waypoint terakhir dan loop true, kita wrap ke waypoint 0 dengan sisa progress.
                // Tapi kita perlu menghitung ulang progress berdasarkan jarak yang tersisa.
                // Untuk menyederhanakan, kita akan gunakan metode total panjang track.
                // Karena implementasi ini sederhana, kita ubah pendekatan: gunakan total distance.
            }
        }
        else if (segmentProgress < 0f)
        {
            // Mundur melewati segmen sebelumnya
            int prevIndex = (currentSegment - 1 + waypoints.Count) % waypoints.Count;
            currentSegment = prevIndex;
            segmentProgress += 1f; // karena negatif, tambah 1 untuk pindah ke segmen sebelumnya
            if (!loop && currentSegment == 0)
            {
                // Di awal, stop
                currentSpeed = 0f;
                segmentProgress = 0f;
                transform.position = waypoints[0].position;
                return;
            }
        }

        // Interpolasi posisi antara waypoint[currentSegment] dan waypoint[nextIndex]
        nextIndex = (currentSegment + 1) % waypoints.Count;
        Vector3 posA = waypoints[currentSegment].position;
        Vector3 posB = waypoints[nextIndex].position;
        // Jika loop, jarak antara waypoint terakhir dan pertama dihitung normal
        Vector3 newPos = Vector3.Lerp(posA, posB, segmentProgress);
        transform.position = newPos;
    }

    private void UpdateRotation()
    {
        if (waypoints.Count < 2) return;
        int nextIndex = (currentSegment + 1) % waypoints.Count;
        Vector3 dir = (waypoints[nextIndex].position - waypoints[currentSegment].position).normalized;
        // Jika progress mendekati 1, arah ke segmen berikutnya
        // Untuk lebih smooth, kita gunakan arah rata-rata atau arah sesaat di titik tersebut
        // Kita bisa menggunakan arah berdasarkan posisi sekarang dan posisi sedikit ke depan
        Vector3 forwardDir = Vector3.zero;
        // Ambil titik di depan sejauh 0.1 progress
        float lookAhead = 0.05f;
        float prog = Mathf.Clamp01(segmentProgress + lookAhead);
        int seg = currentSegment;
        if (prog >= 1f)
        {
            seg = (currentSegment + 1) % waypoints.Count;
            prog -= 1f;
        }
        int nextIdx = (seg + 1) % waypoints.Count;
        Vector3 posNow = transform.position;
        Vector3 posAhead = Vector3.Lerp(waypoints[seg].position, waypoints[nextIdx].position, prog);
        forwardDir = (posAhead - posNow).normalized;
        if (forwardDir.sqrMagnitude < 0.001f)
        {
            // Jika terlalu dekat, gunakan arah dari segmen saat ini
            forwardDir = (waypoints[nextIndex].position - waypoints[currentSegment].position).normalized;
        }

        Quaternion targetRot = Quaternion.LookRotation(forwardDir, Vector3.up);
        // Slerp halus
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, smoothing * Time.fixedDeltaTime);
        // Kemiringan (tilt) opsional
        float angle = Vector3.SignedAngle(transform.forward, forwardDir, Vector3.up);
        float tilt = -angle * tiltAngle * 0.1f;
        Quaternion tiltRot = Quaternion.AngleAxis(tilt, transform.forward);
        rb.rotation *= tiltRot;
    }

    // Opsional: visualisasi track di editor
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
        if (loop && waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
    }
}