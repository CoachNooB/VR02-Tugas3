using UnityEngine;

/// <summary>
/// Simple First Person Controller for UAS Horror Zone.
/// Handles WASD movement, mouse look, and jump.
/// Does NOT depend on Easy FPS or any tags.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class UAS_SimpleFPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalVelocity;
    private float cameraPitch = 0f;
    private bool cursorLocked = true;

    // Link to Horror System for Dialogue pauses
    private UAS_HorrorSystem horrorSystem;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Create camera if not found as child
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(transform);
            camObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            camObj.transform.localRotation = Quaternion.identity;
            playerCamera = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-link Horror System
        horrorSystem = FindAnyObjectByType<UAS_HorrorSystem>();
    }

    void Update()
    {
        // Freeze movement/looking when in dialogue
        if (horrorSystem != null && horrorSystem.IsInDialogue)
        {
            return;
        }

        HandleMouseLook();
        HandleMovement();
        HandleCursorLock();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation (rotate the whole player)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (rotate only the camera)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Speed
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Move direction relative to player facing
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !cursorLocked;
        }
    }
}
