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
                statusText.text = "Peringatan: Struktur pintu melemah! Cari pistol di laci!";
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            if (statusText != null)
                statusText.text = "Kembali ke laci untuk mengambil pistol.";
        }
    }
}