using UnityEngine;

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
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null) _cameraTransform = cam.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Mouse Look
        if (_cameraTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);
            Vector3 camRot = _cameraTransform.localEulerAngles;
            float pitch = camRot.x - mouseY;
            if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch, -85f, 85f);
            _cameraTransform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        // Gerakan
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move.y = 0;
        move.Normalize();
        move *= _walkSpeed;

        // Gravitasi & Lompat
        if (_controller.isGrounded && _velocity.y < 0) _velocity.y = -2f;
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

        _velocity.y += _gravity * Time.deltaTime;
        move += _velocity;
        _controller.Move(move * Time.deltaTime);
    }
}