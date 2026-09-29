using System.Collections;
using TMPro;
using UnityEngine;

public class UINotificationPanelPrefab : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float autoDestroyTime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine lifetimeCoroutine;
    private UINotification centralManager;

    private void OnEnable()
    {
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    public void Setup(string message, Color color, UINotification manager)
    {
        centralManager = manager;

        if (messageText == null || canvasGroup == null)
            return;

        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }

        // Reset visual state
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        transform.localScale = Vector3.one;

        // Stop any previous instance's routine
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        // Safely start lifetime routine
        if (gameObject.activeInHierarchy) lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    public void ForceDismiss()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        if (gameObject.activeInHierarchy)
            StartCoroutine(FadeAndDestroyRoutine());
        else
        {
            // If inactive, unregister and return directly to pool without coroutines
            CleanupAndPool();
        }
    }

    private IEnumerator LifetimeRoutine()
    {
        // FIX: Use WaitForSecondsRealtime so pausing/timeScale=0 doesn't freeze the timer!
        yield return new WaitForSecondsRealtime(autoDestroyTime);
        yield return FadeAndDestroyRoutine();
    }

    private IEnumerator FadeAndDestroyRoutine()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < fadeDuration)
        {
            // FIX: Use unscaledDeltaTime for smooth fading regardless of timeScale
            elapsed += Time.unscaledDeltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        CleanupAndPool();
    }

    private void CleanupAndPool()
    {
        if (centralManager != null)
            centralManager.UnregisterPanel(this);

        Pool.Destroy(gameObject);
    }
}