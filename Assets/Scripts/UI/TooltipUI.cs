using TMPro;
using UnityEngine;
using UnityEngine.UI; // Required for LayoutRebuilder

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
    [SerializeField] private bool clampToScreen = true;

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
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
            canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
        else
        {
            Debug.LogError("[TooltipUI] No Canvas found! The TooltipUI script must be a child of a Canvas.");
            enabled = false;
            return;
        }

        if (tooltipPanel != null)
        {
            // --- FLICKER FIX ---
            // Disable Raycast Target on the panel background and the text
            // so the tooltip doesn't block the mouse from interacting with the icon.
            var graphics = tooltipPanel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var graphic in graphics)
            {
                graphic.raycastTarget = false;
            }

            tooltipPanel.gameObject.SetActive(false);
        }
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

    public void Show(string text, Vector2? screenPosition = null)
    {
        if (tooltipPanel == null || labelText == null) return;

        labelText.text = text;
        tooltipPanel.gameObject.SetActive(true);
        isVisible = true;

        // Force layout rebuild so width/height are accurate for clamping
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);

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
        if (Pointer.current != null)
            return Pointer.current.position.ReadValue();
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

        // 1. Convert screen position to local space in the canvas
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPos, canvasCamera, out Vector2 localPoint))
            return;

        // 2. Apply offset
        localPoint += cursorOffset;

        // 3. Clamping (Ensures the tooltip doesn't go off-screen)
        if (clampToScreen)
        {
            float width = tooltipPanel.rect.width;
            float height = tooltipPanel.rect.height;

            // We clamp based on the canvas bounds
            localPoint.x = Mathf.Clamp(localPoint.x, canvasRect.rect.xMin, canvasRect.rect.xMax - width);
            localPoint.y = Mathf.Clamp(localPoint.y, canvasRect.rect.yMin + height, canvasRect.rect.yMax);
        }

        // 4. Convert back to World Space and apply
        tooltipPanel.position = canvasRect.TransformPoint(localPoint);
    }
}