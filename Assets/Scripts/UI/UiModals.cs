using UnityEngine;

public class UiModals : MonoBehaviour
{
    [Header("Modal Visual Settings")]
    [SerializeField] private GameObject ModalPrefab;
    
    
    protected void Awake()
    {
        Initialize();
    }
    
    protected virtual void Initialize()
    {
        
    }
    
    /// Shows or hides the panel without disabling the component itself.
    public virtual void SetModalActive(bool isActive)
    {
        if (ModalPrefab != null)
            ModalPrefab.SetActive(isActive);
    }
}