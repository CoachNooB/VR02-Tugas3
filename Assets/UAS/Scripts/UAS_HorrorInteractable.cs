using UnityEngine;
using UnityEngine.Events;

public class UAS_HorrorInteractable : MonoBehaviour
{
    [Header("Interactable Settings")]
    public string objectName = "Interactable Object";
    public bool isInteractable = true;
    
    [Header("Bonus Challenge")]
    [Tooltip("If true, this object can only be interacted with after the player has entered a specific trigger zone.")]
    public bool requiresTriggerZone = false;
    
    [Header("Visual Feedback")]
    public Renderer objectRenderer;
    public Color highlightColor = Color.red; // Eerie red for horror theme
    private Color originalColor;
    private bool isHighlighted = false;

    [Header("Dialogue System")]
    [Tooltip("Check this if this object is a ghost/NPC that has dialogue lines.")]
    public bool isGhostNPC = false;
    [TextArea(3, 10)]
    public string[] dialogueLines = new string[] {
        "Hantu: Berani sekali kamu memasuki wilayahku...",
        "Hantu: Rumah ini dikutuk oleh kekuatan kegelapan.",
        "Hantu: Temukan rahasia di dalam peti mati untuk menyelamatkan diri!"
    };

    [Header("Spooky Effects")]
    public bool vanishAfterInteract = false;
    public float vanishDelay = 0.5f;
    public GameObject particlesPrefab;
    public AudioSource soundEffect;

    [Header("Ghost Floating Movement")]
    public bool floatAndRotate = true;
    public float floatSpeed = 1.5f;
    public float floatHeight = 0.15f;
    public float rotateSpeed = 20f;

    [Header("Interaction Action")]
    public UnityEvent onInteract;
    
    private bool hasInteracted = false;
    public bool HasInteracted => hasInteracted;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;

        if (objectRenderer == null)
            objectRenderer = GetComponent<Renderer>();
            
        if (objectRenderer != null && objectRenderer.material != null)
        {
            // Cache the original color
            if (objectRenderer.material.HasProperty("_Color"))
                originalColor = objectRenderer.material.color;
            else if (objectRenderer.material.HasProperty("_BaseColor"))
                originalColor = objectRenderer.material.GetColor("_BaseColor");
            else
                originalColor = Color.white;
        }

        // Auto-configure AudioSource if attached
        if (soundEffect == null)
        {
            soundEffect = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Spooky floating and rotating animation in place
        if (floatAndRotate && (!hasInteracted || !vanishAfterInteract))
        {
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    public void SetHighlight(bool highlight)
    {
        if (!isInteractable || hasInteracted || objectRenderer == null) return;

        isHighlighted = highlight;
        Color targetColor = highlight ? highlightColor : originalColor;

        if (objectRenderer.material.HasProperty("_Color"))
            objectRenderer.material.color = targetColor;
        else if (objectRenderer.material.HasProperty("_BaseColor"))
            objectRenderer.material.SetColor("_BaseColor", targetColor);
    }

    public bool Interact(bool triggerZoneActive)
    {
        if (!isInteractable || hasInteracted) return false;

        // Bonus Challenge: Check if trigger zone condition is met
        if (requiresTriggerZone && !triggerZoneActive)
        {
            Debug.Log($"[Horror] Cannot interact with {objectName} yet. Need to trigger the zone first!");
            return false;
        }

        // Play Sound
        if (soundEffect != null)
        {
            soundEffect.Play();
        }

        // Invoke custom actions
        if (onInteract != null)
        {
            onInteract.Invoke();
        }

        // If it's a dialog NPC, the HorrorSystem handles dialogue progression.
        // We only mark it as interacted and trigger special effects when dialogue finishes.
        if (isGhostNPC && dialogueLines != null && dialogueLines.Length > 0)
        {
            return true; 
        }

        CompleteInteraction();
        return true;
    }

    public void CompleteInteraction()
    {
        hasInteracted = true;
        
        // Change color to green or turn off highlight
        if (objectRenderer != null)
        {
            Color successColor = Color.green;
            if (objectRenderer.material.HasProperty("_Color"))
                objectRenderer.material.color = successColor;
            else if (objectRenderer.material.HasProperty("_BaseColor"))
                objectRenderer.material.SetColor("_BaseColor", successColor);
        }

        if (vanishAfterInteract)
        {
            // Spawn particles
            if (particlesPrefab != null)
            {
                GameObject p = Instantiate(particlesPrefab, transform.position, transform.rotation);
                Destroy(p, 3f);
            }
            
            // Disable mesh renderer and collider to make it vanish spookily, then destroy
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = false;
            
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var c in colliders) c.enabled = false;

            Destroy(gameObject, vanishDelay + 0.1f);
        }

        Debug.Log($"[Horror] Successfully completed interaction with: {objectName}");
    }
    
    public void ResetInteractable()
    {
        hasInteracted = false;
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = true;
        
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = true;
        
        SetHighlight(false);
    }
}
