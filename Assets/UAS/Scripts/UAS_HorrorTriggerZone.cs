using UnityEngine;

public class UAS_HorrorTriggerZone : MonoBehaviour
{
    [Header("Trigger Zone Settings")]
    public string zoneName = "Trigger Zone Horror";
    
    [Tooltip("Reference to the UAS_HorrorSystem. If null, will search for it automatically.")]
    public UAS_HorrorSystem horrorSystem;

    private void Start()
    {
        if (horrorSystem == null)
        {
            horrorSystem = FindAnyObjectByType<UAS_HorrorSystem>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player (either via controller, tag, or name)
        if (other.CompareTag("Player") || other.GetComponent<HalimahFirstPersonController>() != null || other.GetComponent<PlayerMovement>() != null)
        {
            if (horrorSystem != null)
            {
                horrorSystem.SetPlayerInTriggerZone(true, zoneName);
            }
            else
            {
                Debug.LogWarning($"[HorrorTriggerZone] Player entered, but UAS_HorrorSystem reference is missing on {gameObject.name}!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<HalimahFirstPersonController>() != null || other.GetComponent<PlayerMovement>() != null)
        {
            if (horrorSystem != null)
            {
                horrorSystem.SetPlayerInTriggerZone(false, zoneName);
            }
        }
    }
}
