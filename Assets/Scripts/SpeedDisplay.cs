using UnityEngine;
using TMPro;

// Menampilkan kecepatan mobil (km/jam) di layar.
public class SpeedDisplay : MonoBehaviour
{
    public Rigidbody carRigidbody;     // Rigidbody mobil
    public TextMeshProUGUI speedText;

    private void Update()
    {
        if (carRigidbody == null || speedText == null)
            return;

        // linearVelocity satuannya meter/detik. Dikali 3.6 -> km/jam.
        float kmh = carRigidbody.linearVelocity.magnitude * 3.6f;
        speedText.text = Mathf.RoundToInt(kmh) + " km/jam";
    }
}
