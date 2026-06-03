using UnityEngine;
using UnityEngine.InputSystem;

public class CarFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 3f, -7f);
    public float followSpeed = 10f;
    public float lookHeight = 1.2f;

    [Header("Mouse Look")]
    private bool cameraControlEnabled;
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

    private void Start()
    {
        mouse = Mouse.current;

        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }

        SetCameraControl(false);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (cameraControlEnabled)
        {
            ReadMouseLook();
            AutoResetCamera();
        }

        FollowTarget();
    }

    private void ReadMouseLook()
    {
        if (mouse == null)
        {
            mouse = Mouse.current;
        }

        if (mouse == null)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();

        if (mouseDelta.sqrMagnitude < 0.01f)
        {
            return;
        }

        yaw += mouseDelta.x * mouseSensitivity;
        pitch -= mouseDelta.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        lastMouseMoveTime = Time.time;
    }

    private void AutoResetCamera()
    {
        if (!autoResetBehindCar)
        {
            return;
        }

        bool mouseRecentlyMoved = Time.time - lastMouseMoveTime < resetDelay;

        if (mouseRecentlyMoved)
        {
            return;
        }

        float targetYaw = target.eulerAngles.y;

        yaw = Mathf.LerpAngle(
            yaw,
            targetYaw,
            resetSpeed * Time.deltaTime
        );
    }

    private void FollowTarget()
    {
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition = target.position + cameraRotation * offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;

        transform.LookAt(lookTarget);
    }
    public void SetCameraControl(bool enabled)
    {
        cameraControlEnabled = enabled;

        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enabled;
    }
}