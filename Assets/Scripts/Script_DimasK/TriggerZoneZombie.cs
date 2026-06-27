using TMPro;
using UnityEngine;

public class TriggerZoneZombie : MonoBehaviour 
{
    public TextMeshProUGUI statusText;
    public static bool hasInspectedDoor = false; 

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            hasInspectedDoor = true;
            if (statusText != null)
                statusText.text = "⚠️ PINTU DARURAT TERBUKA! Ambil pistol di laci segera!";
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            if (statusText != null)
                statusText.text = "Kembalilah ke laci untuk mengambil pistol!";
        }
    }
}