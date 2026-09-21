using System.Collections;
using TMPro;
using UnityEngine;

public class UINotificationPanelPrefab : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float autoDestroyTime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private TextMeshProUGUI messageText;
    private CanvasGroup canvasGroup;

    private Coroutine lifetimeCoroutine;
    private Coroutine fadeCoroutine;
    private UINotification centralManager;

    public void Setup(string message, Color color, UINotification manager)
    {
        centralManager = manager;

        if (messageText == null)
            messageText = GetComponentInChildren<TextMeshProUGUI>();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }

        // Reset opacity to maximum for pooled reuse
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Reset transform scale
        transform.localScale = Vector3.one;

        // Clear active coroutines from previous pool cycles
        StopAllCoroutines();

        // Safely start lifetime routine if active
        if (gameObject.activeInHierarchy)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
        }
    }

    public void ForceDismiss()
    {
        StopAllCoroutines();

        if (gameObject.activeInHierarchy)
        {
            fadeCoroutine = StartCoroutine(FadeAndDestroyRoutine());
        }
        else
        {
            // If inactive, unregister and return directly to pool without coroutines
            if (centralManager != null)
                centralManager.UnregisterPanel(this);

            Pool.Destroy(gameObject);
        }
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

        Pool.Destroy(gameObject);
    }
}