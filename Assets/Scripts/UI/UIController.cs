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
        UiModals[] foundModals = GetComponents<UiModals>();

        foreach (var modal in foundModals)
        {
            Type modalType = modal.GetType();
            if (!modalDictionary.ContainsKey(modalType)) 
                modalDictionary.Add(modalType, modal);
        }

        PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (!prop.CanWrite) continue;

            foreach (var modal in foundModals)
            {
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

        CloseAllModals();

        if (modalDictionary.TryGetValue(targetType, out UiModals targetModal))
        {
            if (targetModal != null) 
            {
                targetModal.SetModalActive(true);
                if (targetType != typeof(UIHUD)) 
                    SetMovementEnabled(false);
            }
        }
        else
        {
            Debug.LogWarning($"[UIController] Tried to open modal of type {targetType.Name}, but it wasn't found on the UIController GameObject.");
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
            GameManager.Instance.InputManager.SetMovementActive(isEnabled);
    }
}