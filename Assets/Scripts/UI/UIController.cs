using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private Dictionary<Type, UiModals> modalDictionary = new();

    private void Awake() => InitializeModals();

    private void Start()
    {
        // Subscribe in Start to ensure GameManager and InputManager are fully initialized
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null) 
            GameManager.Instance.InputManager.OnEscape.AddListener(CloseAllModals);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null) 
            GameManager.Instance.InputManager.OnEscape.RemoveListener(CloseAllModals);
    }
    
    private void InitializeModals()
    {
        modalDictionary.Clear();
        UiModals[] foundModals = GetComponents<UiModals>();

        foreach (var modal in foundModals)
        {
            Type modalType = modal.GetType();
            if (!modalDictionary.ContainsKey(modalType)) 
                modalDictionary.Add(modalType, modal);
        }
    }
    
    public void OpenModal<T>() where T : UiModals
    {
        Type targetType = typeof(T);

        // Deactivate all pop-up modals first (leaving UIHUD alone)
        CloseAllModals();

        // Activate the target modal if found
        if (modalDictionary.TryGetValue(targetType, out UiModals targetModal))
        {
            if (targetModal != null) 
                targetModal.SetModalActive(true);
        }
        else
            Debug.LogWarning($"[UIController] Tried to open modal of type {targetType.Name}, but it wasn't found on the UIController GameObject.");
    }

    public void CloseAllModals()
    {
        foreach (var modal in modalDictionary.Values)
        {
            // Skip UIHUD so it remains permanently active across all screens
            if (modal != null && modal is not UIHUD) 
                modal.SetModalActive(false);
        }
    }
}