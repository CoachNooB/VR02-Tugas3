using UnityEngine;
using UnityEngine.InputSystem;

public class TrainFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 3f, -7f);
    public float followSpeed = 10f;
    public float lookHeight = 1.2f;

    [Header("Tunnel Settings")]
    public Vector3 tunnelOffset = new Vector3(0f, 1.5f, -4f);
    public float tunnelLookHeight = 0.5f;
    private Vector3 originalOffset;
    private float originalLookHeight;
    private bool isInTunnel = false;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.15f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Auto Reset Behind Car")]
    public bool autoResetBehindCar = true;
    public float resetDelay = 2f;
    public float resetSpeed = 3f;

    private float yaw;
    private float pitch = 15f;
    private float lastMouseMoveTime;
    private Mouse mouse;
    private bool cameraControlEnabled = false; // Default: nonaktif

    private void Start()
    {
        mouse = Mouse.current;
        if (target != null) yaw = target.eulerAngles.y;

        // Kursor bebas di awal
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        originalOffset = offset;
        originalLookHeight = lookHeight;
    }

    private void Update()
    {
        // Klik kiri untuk mengaktifkan kontrol mouse
        if (Input.GetMouseButtonDown(0) && !cameraControlEnabled)
        {
            SetCameraControl(true);
        }
        // ESC untuk menonaktifkan kontrol mouse
        if (Input.GetKeyDown(KeyCode.Escape) && cameraControlEnabled)
        {
            SetCameraControl(false);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (cameraControlEnabled)
        {
            ReadMouseLook();
            AutoResetCamera();
        }

        FollowTarget();
    }

    private void ReadMouseLook()
    {
        if (mouse == null) mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        if (mouseDelta.sqrMagnitude < 0.01f) return;

        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        lastMouseMoveTime = Time.time;
    }

    private void AutoResetCamera()
    {
        if (!autoResetBehindCar) return;
        if (Time.time - lastMouseMoveTime < resetDelay) return;

        float targetYaw = target.eulerAngles.y;
        yaw = Mathf.LerpAngle(yaw, targetYaw, resetSpeed * Time.deltaTime);
    }

    private void FollowTarget()
    {
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + cameraRotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        transform.LookAt(lookTarget);
    }

    public void SetCameraControl(bool enabled)
    {
        cameraControlEnabled = enabled;
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
        if (enabled) lastMouseMoveTime = Time.time; // Reset timer agar tidak langsung auto-reset
    }

    public void SetTunnel(bool active)
    {
        if (active && !isInTunnel)
        {
            isInTunnel = true;
            offset = tunnelOffset;
            lookHeight = tunnelLookHeight;
        }
        else if (!active && isInTunnel)
        {
            isInTunnel = false;
            offset = originalOffset;
            lookHeight = originalLookHeight;
        }
    }
}