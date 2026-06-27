using TMPro;
using UnityEngine;

public class PlayerSystemController : MonoBehaviour 
{
    [Header("Raycast Config")]
    public Transform cameraTransform;
    public float rayDistance = 5f;
    
    [Header("Layer Masks")]
    public LayerMask interactableLayerMask;
    public LayerMask zombiePushableLayerMask;

    [Header("Physics")]
    public float bulletImpactForce = 15f;

    [Header("UI & References")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI ammoText;
    public InteractableObject weaponDrawerScript;

    [Header("Ammo")]
    public int maxAmmo = 10;
    private int _currentAmmo;

    private InteractableObject _currentHoveredObject;
    private bool _hasPistol = false;

    private void Awake()
    {
        if (cameraTransform == null) cameraTransform = transform;
        _currentAmmo = maxAmmo;
        
        if (statusText == null)
            statusText = FindAnyObjectByType<TextMeshProUGUI>();

        if (weaponDrawerScript == null)
        {
            InteractableObject[] objs = FindObjectsByType<InteractableObject>(FindObjectsInactive.Include);
            foreach (var obj in objs)
                if (obj.IsWeaponDrawer) { weaponDrawerScript = obj; break; }
        }

        UpdateAmmoUI();
    }

    private void Update()
    {
        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        // ===== RAYCAST HIGHLIGHT & INTERAKSI =====
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance, interactableLayerMask))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();
            if (obj != null)
            {
                if (_currentHoveredObject != obj)
                {
                    if (_currentHoveredObject != null) _currentHoveredObject.SetHighlight(false);
                    _currentHoveredObject = obj;
                    _currentHoveredObject.SetHighlight(true);
                }

                if (obj.IsWeaponDrawer && !TriggerZoneZombie.hasInspectedDoor)
                    statusText.text = "[TERKUNCI] Laci terkunci! Periksa pintu Ruang 1 dulu.";
                else
                    statusText.text = "Melihat: " + obj.name;

                promptText.text = "[Tekan E] untuk interaksi";

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (_currentHoveredObject.Interact())
                    {
                        statusText.text = "Berinteraksi dengan " + _currentHoveredObject.name + "!";
                        if (_currentHoveredObject == weaponDrawerScript)
                        {
                            _hasPistol = true;
                            _currentAmmo = maxAmmo;
                            UpdateAmmoUI();
                            statusText.text = "Pistol diperoleh! (Ammo: " + _currentAmmo + ")";
                        }
                        promptText.text = "";
                    }
                    else
                        statusText.text = "Gagal! (mungkin terkunci)";
                }
            }
        }
        else
        {
            if (_currentHoveredObject != null)
            {
                _currentHoveredObject.SetHighlight(false);
                _currentHoveredObject = null;
                statusText.text = "Jelajahi bunker...";
                promptText.text = "";
            }
        }

        // ===== TEMBAKAN =====
        if (Input.GetMouseButtonDown(0) && _hasPistol && _currentAmmo > 0)
        {
            ShootAndPushZombie(origin, direction);
            _currentAmmo--;
            UpdateAmmoUI();
            if (_currentAmmo == 0)
                statusText.text = "Peluru habis!";
        }
        else if (Input.GetMouseButtonDown(0) && _hasPistol && _currentAmmo == 0)
        {
            statusText.text = "Peluru habis!";
        }
    }

    private void ShootAndPushZombie(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, 20f, zombiePushableLayerMask))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForceAtPosition(direction * bulletImpactForce, hit.point, ForceMode.Impulse);
                statusText.text = "Tembakan kena! Zombie terpental!";

                // Efek percikan sederhana
                GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.transform.position = hit.point;
                spark.transform.localScale = Vector3.one * 0.2f;
                spark.GetComponent<Renderer>().material.color = Color.red;
                Destroy(spark, 1f);
            }
        }
        else
        {
            statusText.text = "Meleset...";
        }
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = _currentAmmo + " / " + maxAmmo;
    }
}