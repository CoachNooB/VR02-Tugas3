using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonPulseEffect : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minScale = 0.95f;
    public float maxScale = 1.05f;

    private Image image;
    private Vector3 originalScale;
    private Color originalColor;

    void Start()
    {
        image = GetComponent<Image>();
        originalScale = transform.localScale;
        if (image != null) originalColor = image.color;
        StartCoroutine(Pulse());
    }

    IEnumerator Pulse()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float lerp = Mathf.PingPong(t, 1f);
            
            // Scale
            float scale = Mathf.Lerp(minScale, maxScale, lerp);
            transform.localScale = originalScale * scale;
            
            yield return null;
        }
    }
}