using UnityEngine;

// Reset mobil ke posisi awal: tekan R, atau otomatis kalau jatuh / terbalik.
public class CarRespawn : MonoBehaviour
{
    [Header("Pengaturan")]
    public float fallY = -5f;              // kalau posisi Y di bawah ini -> respawn
    public KeyCode respawnKey = KeyCode.R;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private void Start()
    {
        // ingat posisi & rotasi awal mobil
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // hampir terbalik? (arah "atas" mobil sudah tidak mengarah ke atas)
        bool terbalik = Vector3.Dot(transform.up, Vector3.up) < 0.2f;
        bool diamSaatTerbalik = terbalik && rb != null && rb.linearVelocity.magnitude < 1f;

        if (Input.GetKeyDown(respawnKey) || transform.position.y < fallY || diamSaatTerbalik)
            Respawn();
    }

    private void Respawn()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
