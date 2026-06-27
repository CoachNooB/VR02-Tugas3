using UnityEngine;

public class InteractableObject : MonoBehaviour 
{
    [SerializeField] private Renderer _objectRenderer;
    [SerializeField] private GameObject _pistolInHand;   
    [SerializeField] private bool _isWeaponDrawer = false; 
    [SerializeField] private bool _isAmmoPickup = false;
    [SerializeField] private bool _isHealthPickup = false;
    [SerializeField] private int _ammoAmount = 5;
    [SerializeField] private int _healthAmount = 20;

    private Color _normalColor;
    private Color _highlightColor = Color.yellow;
    private bool _hasInteracted = false;

    public bool IsInteracted => _hasInteracted;
    public bool IsWeaponDrawer => _isWeaponDrawer;
    public bool IsAmmoPickup => _isAmmoPickup;
    public bool IsHealthPickup => _isHealthPickup;
    public int AmmoAmount => _ammoAmount;
    public int HealthAmount => _healthAmount;

    private void Awake() 
    {
        if (_objectRenderer == null) _objectRenderer = GetComponent<Renderer>();
        if (_objectRenderer != null)
            _normalColor = _objectRenderer.material.color;
        else
            Debug.LogError($"InteractableObject {name} tidak punya Renderer!");
    }

    public void SetHighlight(bool highlight) 
    {
        if (_hasInteracted || _objectRenderer == null) return; 

        if (_isWeaponDrawer && !TriggerZoneZombie.hasInspectedDoor) 
        {
            _objectRenderer.material.color = Color.red;
            return;
        }

        _objectRenderer.material.color = highlight ? _highlightColor : _normalColor;
    }

    public bool Interact() 
    {
        if (_hasInteracted || _objectRenderer == null) return false;
        if (_isWeaponDrawer && !TriggerZoneZombie.hasInspectedDoor) 
            return false;

        _hasInteracted = true;
        _objectRenderer.material.color = Color.green; 

        if (_isWeaponDrawer && _pistolInHand != null) 
        {
            _pistolInHand.SetActive(true);
        }

        return true;
    }

    public void InitializeAsWeaponDrawer(GameObject pistol)
    {
        _isWeaponDrawer = true;
        _pistolInHand = pistol;
    }

    // Method untuk ammo pickup
    public void SetAsAmmoPickup(int amount = 5)
    {
        _isAmmoPickup = true;
        _ammoAmount = amount;
        // Ubah warna menjadi kuning
        if (_objectRenderer != null)
            _objectRenderer.material.color = Color.yellow;
    }

    // Method untuk health pickup
    public void SetAsHealthPickup(int amount = 20)
    {
        _isHealthPickup = true;
        _healthAmount = amount;
        // Ubah warna menjadi merah muda / pink
        if (_objectRenderer != null)
            _objectRenderer.material.color = new Color(1f, 0.4f, 0.6f);
    }
}