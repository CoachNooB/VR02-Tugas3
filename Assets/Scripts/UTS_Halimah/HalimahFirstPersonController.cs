using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HalimahFirstPersonController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public CharacterController characterController;

    [Header("Settings")]
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;
    
    private float verticalRotation = 0f;

    private void Start()
    {
        // Safely find CharacterController
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = GetComponentInChildren<CharacterController>();
        if (characterController == null)
            characterController = FindAnyObjectByType<CharacterController>();

        // Safely find Player Camera
        if (playerCamera == null && GetComponentInChildren<Camera>() != null)
            playerCamera = GetComponentInChildren<Camera>().transform;
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
        if (playerCamera == null)
        {
            Camera cam = FindAnyObjectByType<Camera>();
            if (cam != null)
                playerCamera = cam.transform;
        }

        // Log warnings if still missing
        if (characterController == null)
        {
            Debug.LogWarning("HalimahFirstPersonController: CharacterController is missing! Please attach a CharacterController to this GameObject.");
        }
        if (playerCamera == null)
        {
            Debug.LogWarning("HalimahFirstPersonController: Player Camera is missing! Please assign the Main Camera to the playerCamera field.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Mouse look
        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        // If New Input System is active, read delta from Mouse.current
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            mouseX = mouseDelta.x * mouseSensitivity * 0.05f; // Scale down because delta is pixel-based
            mouseY = mouseDelta.y * mouseSensitivity * 0.05f;
        }
        else
        {
            // Fallback to legacy in case Mouse.current is null
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
#else
        // Legacy Input
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
#endif

        if (playerCamera != null)
        {
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);

        // Movement (WASD / Arrow keys)
        float horizontal = 0f;
        float vertical = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        }
        else
        {
            // Fallback to legacy
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
#else
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
#endif

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        if (move.magnitude > 1f)
            move.Normalize();

        if (characterController != null && characterController.enabled)
        {
            // Apply walk speed and deltaTime
            characterController.Move(move * walkSpeed * Time.deltaTime);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
