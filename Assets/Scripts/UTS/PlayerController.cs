using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Pergerakan WASD")]
    public float moveSpeed = 5f;

    [Header("Kamera & Mouse Look")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    public float upperLookLimit = -80f;
    public float lowerLookLimit = 80f;

    [Header("Sistem Interaksi")]
    public float interactDistance = 3f;

    [Header("Crosshair UI Optional")]
    public Image crosshairImage;

    private CharacterController characterController;
    private float rotationX = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairImage != null)
        {
            crosshairImage.raycastTarget = false;
        }
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();

        if (Input.GetMouseButtonDown(0))
        {
            TryInteractFromCrosshair();
        }
    }

    void HandleRotation()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, upperLookLimit, lowerLookLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * moveZ + transform.right * moveX;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void TryInteractFromCrosshair()
    {
        if (playerCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // First: try UI interaction, for World Space Canvas buttons
        if (TryClickUI(screenCenter))
        {
            return;
        }

        // Second: optional fallback for normal 3D objects with colliders
        TryClickWorldObject();
    }

    bool TryClickUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("Tidak ada EventSystem di scene.");
            return false;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponentInParent<Button>();

            if (button != null && button.interactable)
            {
                ExecuteEvents.Execute(
                    button.gameObject,
                    pointerData,
                    ExecuteEvents.pointerClickHandler
                );

                Debug.Log("Berhasil menekan tombol UI: " + button.name);
                return true;
            }
        }

        return false;
    }

    void TryClickWorldObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                return;
            }

            Debug.Log("Laser mengenai object: " + hit.collider.name);

            Button button = hit.collider.GetComponentInChildren<Button>();
            if (button == null)
            {
                button = hit.collider.GetComponentInParent<Button>();
            }

            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
                Debug.Log("Berhasil menekan tombol dari collider: " + button.name);
            }
        }
    }
}