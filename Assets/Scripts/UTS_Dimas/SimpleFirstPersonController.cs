using UnityEngine;

public class SimpleFirstPersonController : MonoBehaviour
{
    public Transform playerCamera;
    public CharacterController characterController;
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (playerCamera == null && GetComponentInChildren<Camera>() != null)
            playerCamera = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move.Normalize();
        characterController.Move(move * walkSpeed * Time.deltaTime);
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