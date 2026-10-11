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
        UiModals[] allComponents = GetComponents<UiModals>();

        PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var prop in properties)
        {
            if (!prop.CanWrite || !typeof(UiModals).IsAssignableFrom(prop.PropertyType)) continue;

            UiModals matchingModal = null;

            foreach (var comp in allComponents)
            {
                if (comp != null && prop.PropertyType.IsAssignableFrom(comp.GetType()))
                {
                    matchingModal = comp;
                    break;
                }
            }

            if (matchingModal != null)
            {
                prop.SetValue(this, matchingModal);

                Type modalType = matchingModal.GetType();
                if (!modalDictionary.ContainsKey(modalType))
                {
                    modalDictionary.Add(modalType, matchingModal);
                }
            }
            else
            {
                Debug.LogWarning($"[UIController] Public property '{prop.Name}' was declared, but no matching UiModals component was found on the GameObject!");
            }
        }
    }

    // Helper using the explicit IsOpen boolean tracker
    public UiModals GetCurrentlyOpenModal()
    {
        foreach (var kvp in modalDictionary)
        {
            if (kvp.Key != typeof(UIHUD) && kvp.Value != null && kvp.Value.IsOpen)
            {
                return kvp.Value;
            }
        }
        return null;
    }
    
    public void OpenModal<T>() where T : UiModals
    {
        Type targetType = typeof(T);

        UiModals openModal = GetCurrentlyOpenModal();
        if (targetType != typeof(UIHUD) && openModal != null)
        {
            Debug.LogWarning($"[UIController] Cannot open {targetType.Name}. '{openModal.GetType().Name}' is already active and blocking input!");
            return;
        }

        CloseAllModals();

        if (modalDictionary.TryGetValue(targetType, out UiModals targetModal))
        {
            if (targetModal != null) 
            {
                targetModal.SetModalActive(true);

                if (targetType != typeof(UIHUD))
                {
                    SetMovementEnabled(false);
                }
            }
        }
        else
        {
            Debug.LogWarning($"[UIController] Tried to open modal of type {targetType.Name}, but it wasn't found or declared on the UIController.");
        }
    }

    public void CloseAllModals()
    {
        foreach (var modal in modalDictionary.Values)
        {
            if (modal != null && modal is not UIHUD) 
                modal.SetModalActive(false);
        }

        SetMovementEnabled(true);
    }

    private void SetMovementEnabled(bool isEnabled)
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
        {
            GameManager.Instance.InputManager.SetMovementActive(isEnabled);
        }
    }
}