using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonClickEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public float hoverScale = 1.1f;
    public float pressScale = 0.95f;
    public float duration = 0.1f;
    public Color hoverColor = new Color(0.4f, 0.8f, 1f);
    public Color pressColor = new Color(0.1f, 0.4f, 0.7f);

    private Image image;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;
    private bool isPressed = false;

    void Start()
    {
        image = GetComponent<Image>();
        originalScale = transform.localScale;
        if (image != null) originalColor = image.color;
    }

    void Update()
    {
        Vector3 targetScale = originalScale;
        if (isPressed)
            targetScale = originalScale * pressScale;
        else if (isHovering)
            targetScale = originalScale * hoverScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime / duration);

        if (image != null)
        {
            Color targetColor = originalColor;
            if (isPressed)
                targetColor = pressColor;
            else if (isHovering)
                targetColor = hoverColor;
            image.color = Color.Lerp(image.color, targetColor, Time.deltaTime / duration);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}