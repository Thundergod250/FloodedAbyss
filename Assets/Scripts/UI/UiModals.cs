using UnityEngine;

public class UiModals : MonoBehaviour
{
    [Header("Modal Visual Settings")]
    [SerializeField] private GameObject ModalPrefab;
    
    private CanvasGroup canvasGroup;

    // Explicit boolean tracker for whether this modal is open
    public bool IsOpen { get; private set; } = false;

    protected virtual void Awake()
    {
        GameObject targetGO = ModalPrefab != null ? ModalPrefab : gameObject;

        if (targetGO != null)
        {
            canvasGroup = targetGO.GetComponent<CanvasGroup>();
            if (canvasGroup == null) 
                Debug.LogWarning($"[UiModals] {GetType().Name}: Target modal object '{targetGO.name}' on '{gameObject.name}' is missing a CanvasGroup component! Falling back to SetActive().", this);
        }
    }

    protected virtual void Start() 
    {
        // Force all modals except UIHUD to start closed
        if (this is not UIHUD)
        {
            SetModalActive(false);
        }
        else
        {
            SetModalActive(true); // HUD stays open by default
        }

        Initialize();
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void Initialize() { }

    public virtual void SetModalActive(bool isActive)
    {
        IsOpen = isActive; // Update our explicit tracker

        GameObject targetGO = ModalPrefab != null ? ModalPrefab : gameObject;

        if (isActive && targetGO != null && !targetGO.activeSelf) 
        {
            targetGO.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isActive ? 1f : 0f;
            canvasGroup.blocksRaycasts = isActive;
            canvasGroup.interactable = isActive;
        }
        else if (targetGO != null)
        {
            targetGO.SetActive(isActive);
        }
    }
}