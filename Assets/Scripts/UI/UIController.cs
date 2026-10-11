using System;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private Dictionary<Type, UiModals> modalDictionary = new();

    // Direct global properties for dot-notation access via GameManager.Instance.uiController.(SpecificUi)
    public UIHUD UiHUD { get; private set; }
    public UiUpgrade UiUpgrade { get; private set; }
    public UIAutoFarm UiAutoFarm { get; private set; }
    public UiApexPump UiApexPump { get; private set; }
    public UiTrade UiTrade { get; private set; }
    public UiResourceHarvest UiResourceHarvest { get; private set; }

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

            // Automatically cache references for direct dot-notation access
            if (modal is UIHUD hud) UiHUD = hud;
            else if (modal is UiUpgrade upgrade) UiUpgrade = upgrade;
            else if (modal is UIAutoFarm autoFarm) UiAutoFarm = autoFarm;
            else if (modal is UiApexPump apexPump) UiApexPump = apexPump;
            else if (modal is UiTrade trade) UiTrade = trade;
            else if (modal is UiResourceHarvest harvest) UiResourceHarvest = harvest; // <-- ADD THIS LINE
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