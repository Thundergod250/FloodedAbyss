using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private Dictionary<Type, UiModals> modalDictionary = new();

    // Developers ONLY need to declare their new UI modal property here:
    public UIHUD UiHUD { get; private set; }
    public UiUpgrade UiUpgrade { get; private set; }
    public UIAutoFarm UiAutoFarm { get; private set; }
    public UiApexPump UiApexPump { get; private set; }
    public UiTrade UiTrade { get; private set; }
    public UiResourceHarvest UiResourceHarvest { get; private set; }
    // Add any future UIs here effortlessly without touching InitializeModals()!

    private void Awake() => InitializeModals();

    private void Start()
    {
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

        // 1. Populate the dictionary
        foreach (var modal in foundModals)
        {
            Type modalType = modal.GetType();
            if (!modalDictionary.ContainsKey(modalType)) 
                modalDictionary.Add(modalType, modal);
        }

        // 2. Automatically bind matching public properties via Reflection
        PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (!prop.CanWrite) continue;

            foreach (var modal in foundModals)
            {
                // If the property type matches or inherits from the modal's type, assign it automatically
                if (prop.PropertyType.IsAssignableFrom(modal.GetType()))
                {
                    prop.SetValue(this, modal);
                    break;
                }
            }
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