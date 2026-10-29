using UnityEngine;
using TMPro;

public class UINotification : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 1.2f;
    
    private RectTransform rectTransform;
    private bool isPulsing = false;

    private void Awake()
    {
        if (notificationText == null)
            notificationText = GetComponent<TextMeshProUGUI>();

        if (notificationText != null)
            rectTransform = notificationText.gameObject.GetComponent<RectTransform>();

        // Start hidden by default
        SetNotificationActive(false, string.Empty, Color.white);
    }

    private void Update()
    {
        // Consistent pulsing effect for any active notification
        if (isPulsing && notificationText != null && notificationText.gameObject.activeSelf && rectTransform != null)
        {
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
            rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    // Public method to be triggered by other scripts (like UINotification_Pump)
    public void SetNotificationActive(bool active, string message = "", Color color = default)
    {
        if (notificationText == null) return;

        isPulsing = active;
        notificationText.gameObject.SetActive(active);

        if (active)
        {
            notificationText.text = message;
            notificationText.color = color == default ? Color.red : color;
        }
        else
        {
            if (rectTransform != null)
                rectTransform.localScale = Vector3.one;
        }
    }
}