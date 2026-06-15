using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Pergerakan (WASD)")]
    public float moveSpeed = 5f;
    
    [Header("Kamera & Mouse Look")]
    public Transform cameraTransform; 
    public float mouseSensitivity = 2f;
    public float upperLookLimit = -80f; 
    public float lowerLookLimit = 80f;  

    [Header("Sistem Interaksi")]
    public float interactDistance = 3f; 

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float rotationX = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Mengunci dan menyembunyikan kursor kustom bawaan OS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();

        if (Input.GetMouseButtonDown(0))
        {
            TryInteractFromCenter();
        }
    }

    void HandleRotation()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotasi Vertikal (Kamera atas/bawah)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, upperLookLimit, lowerLookLimit); 
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Rotasi Horizontal (Player kanan/kiri)
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical");   

        moveDirection = (transform.forward * moveZ) + (transform.right * moveX);
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void TryInteractFromCenter()
    {
        if (cameraTransform == null) return;

        // Memulai tembakan dari kamera lurus ke depan
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        // Garis visual hijau di jendela Scene View
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactDistance, Color.green, 2f);

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Validasi proteksi diri: Jika menabrak badan player sendiri, lewati objeknya
            if (hit.collider.gameObject == this.gameObject || hit.collider.transform.IsChildOf(this.transform))
            {
                Ray secondaryRay = new Ray(hit.point + (cameraTransform.forward * 0.1f), cameraTransform.forward);
                if (!Physics.Raycast(secondaryRay, out hit, interactDistance - hit.distance)) return;
            }

            Debug.Log("Laser Mengenai: " + hit.collider.name);

            // Mencari komponen UI Button pada objek yang tertembak atau relasinya
            Button button = hit.collider.GetComponentInChildren<Button>();
            if (button == null) button = hit.collider.GetComponentInParent<Button>();

            if (button != null && button.interactable)
            {
                button.onClick.Invoke(); 
                Debug.Log("Berhasil menekan tombol: " + button.name);
            }
        }
    }
}