using System.Collections;
using TMPro;
using UnityEngine;

public class UINotificationPanelPrefab : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float autoDestroyTime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    // Private component references fetched automatically
    private TextMeshProUGUI messageText;
    private CanvasGroup canvasGroup;

    private Coroutine lifetimeCoroutine;
    private Coroutine fadeCoroutine;
    private UINotification centralManager;

    public void Setup(string message, Color color, UINotification manager)
    {
        centralManager = manager;

        // Fetch components on setup if not already cached
        if (messageText == null)
            messageText = GetComponentInChildren<TextMeshProUGUI>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }

        // Reset opacity to maximum for pooled reuse
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Stop any active coroutines left over from previous pool usages
        if (lifetimeCoroutine != null) StopCoroutine(lifetimeCoroutine);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    public void ForceDismiss()
    {
        if (lifetimeCoroutine != null)
            StopCoroutine(lifetimeCoroutine);

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeAndDestroyRoutine());
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(autoDestroyTime);
        yield return FadeAndDestroyRoutine();
    }

    private IEnumerator FadeAndDestroyRoutine()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        if (centralManager != null)
            centralManager.UnregisterPanel(this);

        // Return to Object Pool
        Pool.Destroy(gameObject);
    }
}