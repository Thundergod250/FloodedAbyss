using TMPro;
using UnityEngine;

// Import the new Input System namespace if it is enabled
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The panel (background) that is shown/hidden.")]
    [SerializeField] private RectTransform tooltipPanel;
    [Tooltip("The TextMeshProUGUI label inside the panel.")]
    [SerializeField] private TextMeshProUGUI labelText;

    [Header("Positioning")]
    [Tooltip("Offset from the cursor, in canvas units (usually pixels).")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(20f, -20f);
    [SerializeField] private bool followCursor = true;
    [SerializeField] private bool clampToScreen = false;

    private RectTransform canvasRect;
    private Camera canvasCamera;
    private bool isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[TooltipUI] Must be parented under a Canvas.");
            enabled = false;
            return;
        }
        canvasRect = canvas.GetComponent<RectTransform>();
        canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (tooltipPanel != null)
            tooltipPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void LateUpdate()
    {
        if (isVisible && followCursor)
            UpdatePosition(GetCursorPosition());
    }

    // Accept an optional screen position passed from the event system
    public void Show(string text, Vector2? screenPosition = null)
    {
        if (tooltipPanel == null || labelText == null) return;

        labelText.text = text;
        tooltipPanel.gameObject.SetActive(true);
        isVisible = true;

        // Use the provided position, or fallback to getting the current cursor position
        UpdatePosition(screenPosition ?? GetCursorPosition());
    }

    public void Hide()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.gameObject.SetActive(false);
        isVisible = false;
    }

    private Vector2 GetCursorPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector2.zero;
#endif
    }

    private void UpdatePosition(Vector2 screenPos)
    {
        if (canvasRect == null || tooltipPanel == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, canvasCamera, out Vector2 localPoint))
            return;

        localPoint += cursorOffset;

        if (clampToScreen)
        {
            // Assumes tooltipPanel pivot is (0.5, 0.5); adjust if you change it.
            Vector2 half = tooltipPanel.rect.size * 0.5f;
            localPoint.x = Mathf.Clamp(localPoint.x,
                canvasRect.rect.xMin + half.x, canvasRect.rect.xMax - half.x);
            localPoint.y = Mathf.Clamp(localPoint.y,
                canvasRect.rect.yMin + half.y, canvasRect.rect.yMax - half.y);
        }

        tooltipPanel.position = canvasRect.TransformPoint(localPoint);
    }
}