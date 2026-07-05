using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatSpeed = 0.5f;
    public float floatAmplitude = 0.5f;
    public float rotationSpeed = 10f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        // Gerakan naik-turun
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = startPos + Vector3.up * yOffset;

        // Rotasi perlahan
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}