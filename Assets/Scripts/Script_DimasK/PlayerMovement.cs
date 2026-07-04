using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _jumpHeight = 1.5f;
    [Header("Mouse")]
    [SerializeField] private float _mouseSensitivity = 2f;

    private CharacterController _controller;
    private Transform _cameraTransform;
    private Vector3 _velocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (_controller == null)
            _controller = GetComponentInChildren<CharacterController>();
        if (_controller == null)
            _controller = FindAnyObjectByType<CharacterController>();

        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) cam = FindAnyObjectByType<Camera>();
        
        if (cam != null) 
            _cameraTransform = cam.transform;

        if (_controller == null)
        {
            Debug.LogWarning("PlayerMovement: CharacterController is missing!");
        }
        if (_cameraTransform == null)
        {
            Debug.LogWarning("PlayerMovement: Camera is missing!");
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Mouse Look
        float mouseX = 0f;
        float mouseY = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            mouseX = mouseDelta.x * _mouseSensitivity * 0.05f;
            mouseY = mouseDelta.y * _mouseSensitivity * 0.05f;
        }
        else
        {
            mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
            mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
        }
#else
        mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
#endif

        if (_cameraTransform != null)
        {
            transform.Rotate(Vector3.up * mouseX);
            Vector3 camRot = _cameraTransform.localEulerAngles;
            float pitch = camRot.x - mouseY;
            if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch, -85f, 85f);
            _cameraTransform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        // Gerakan
        float x = 0f;
        float z = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrow.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrow.isPressed) z -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrow.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrow.isPressed) x += 1f;
        }
        else
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }
#else
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
#endif

        Vector3 move = transform.right * x + transform.forward * z;
        move.y = 0;
        if (move.magnitude > 1f)
            move.Normalize();
        move *= _walkSpeed;

        // Gravitasi & Lompat
        if (_controller != null && _controller.enabled)
        {
            if (_controller.isGrounded && _velocity.y < 0) 
                _velocity.y = -2f;

            bool isJumpPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                isJumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            }
            else
            {
                isJumpPressed = Input.GetButtonDown("Jump");
            }
#else
            isJumpPressed = Input.GetButtonDown("Jump");
#endif

            if (isJumpPressed && _controller.isGrounded)
                _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

            _velocity.y += _gravity * Time.deltaTime;
            move += _velocity;
            _controller.Move(move * Time.deltaTime);
        }
    }
}