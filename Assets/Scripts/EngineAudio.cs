using UnityEngine;

// Suara mesin yang naik-turun mengikuti kecepatan mobil.
// Butuh komponen AudioSource (otomatis ditambahkan) + klip suara mesin (loop).
[RequireComponent(typeof(AudioSource))]
public class EngineAudio : MonoBehaviour
{
    public Rigidbody carRigidbody;        // Rigidbody mobil
    public float minPitch = 0.6f;         // pitch saat diam
    public float maxPitch = 2.0f;         // pitch saat ngebut
    public float speedForMaxPitch = 40f;  // kecepatan (m/s) untuk pitch maksimum

    private AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        if (!source.isPlaying)
            source.Play();
    }

    private void Update()
    {
        if (carRigidbody == null)
            return;

        float t = Mathf.Clamp01(carRigidbody.linearVelocity.magnitude / speedForMaxPitch);
        source.pitch = Mathf.Lerp(minPitch, maxPitch, t);
    }
}
