using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WarningPopup : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TMP_Text messageText;
    public Image flashImage;
    public float fadeDuration = 0.5f;
    public float displayDuration = 2f;
    private Coroutine activeRoutine;

    public void ShowWarning(string message)
    {
        if (messageText != null) messageText.text = message;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(AnimatePopup());
        StartCoroutine(FlashRed());
    }

    public void ShowSuccess(string message)
    {
        if (messageText != null) messageText.text = message;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(displayDuration);
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    private IEnumerator FlashRed()
    {
        if (flashImage == null) yield break;
        float duration = 0.2f, elapsed = 0f;
        while (elapsed < duration)
        {
            flashImage.color = new Color(1, 0, 0, Mathf.Lerp(0f, 0.6f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            flashImage.color = new Color(1, 0, 0, Mathf.Lerp(0.6f, 0f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        flashImage.color = new Color(1, 0, 0, 0);
    }
}