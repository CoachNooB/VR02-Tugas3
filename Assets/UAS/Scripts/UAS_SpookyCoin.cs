using UnityEngine;
using System.Collections;

public class UAS_SpookyCoin : MonoBehaviour
{
    public static int coinCount = 0;
    
    private bool isCollected = false;
    private float floatSpeed = 3f;
    private float floatHeight = 0.15f;
    private float spinSpeed = 120f;
    
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (isCollected) return;

        // Spinning animation
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);

        // Floating animation
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect Player tag or character controller to avoid double trigger
        if (!isCollected && (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.name.Contains("Player")))
        {
            StartCoroutine(CollectAnimation());
        }
    }

    private IEnumerator CollectAnimation()
    {
        isCollected = true;
        coinCount++;
        
        // Find Horror System to show feedback
        var sys = FindAnyObjectByType<UAS_HorrorSystem>();
        if (sys != null && sys.statusText != null)
        {
            sys.statusText.text = $"Koin Terkumpul! Total: {coinCount}";
        }

        // Collect effect: Spin super fast, move upwards, shrink to zero
        Vector3 initialScale = transform.localScale;
        Vector3 initialPos = transform.position;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Spin faster
            transform.Rotate(Vector3.up * spinSpeed * 5f * Time.deltaTime, Space.World);

            // Rise up
            transform.position = Vector3.Lerp(initialPos, initialPos + Vector3.up * 1.5f, t);

            // Scale down
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
