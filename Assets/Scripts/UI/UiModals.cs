using UnityEngine;

public class UiModals : MonoBehaviour
{
    [Header("Modal Visual Settings")]
    [SerializeField] private GameObject ModalPrefab;
    [SerializeField] private CanvasGroup canvasGroup;

    protected virtual void Start() => Initialize();

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void Initialize() { }
    
    public virtual void SetModalActive(bool isActive)
    {
        GameObject targetGO = ModalPrefab != null ? ModalPrefab : gameObject;
        if (targetGO != null && !targetGO.activeSelf) 
            targetGO.SetActive(true);
        
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