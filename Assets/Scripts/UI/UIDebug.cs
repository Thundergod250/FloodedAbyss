using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDebug : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerResources playerResources;

    [Header("UI Dynamic Layout")]
    [SerializeField] private GameObject debugButtonsPanel;
    [SerializeField] private Transform resourceGridParent;
    [SerializeField] private UIDebugPanel resourceItemPrefab;

    [Header("Input Action")]
    [SerializeField] private InputActionReference debugToggleAction;

    [Header("Resource Debug Buttons")]
    [SerializeField] private Button addResource;

    private Dictionary<ResourceType, UIDebugPanel> uiItemMap = new();

    private void Awake()
    {
        if (playerResources == null) 
            return;

        InitializeUI();
    }

    private void OnEnable()
    {
        if (playerResources != null) 
            playerResources.EvtOnResourceChanged.AddListener(HandleResourceChanged);

        if (debugToggleAction != null)
        {
            debugToggleAction.action.Enable();
            debugToggleAction.action.performed += OnToggleDebug;
        }

        if (addResource != null) 
            addResource.onClick.AddListener(OnClick_AddTenToAllResources);
    }

    private void OnDisable()
    {
        if (playerResources != null) 
            playerResources.EvtOnResourceChanged.RemoveListener(HandleResourceChanged);

        if (debugToggleAction != null)
        {
            debugToggleAction.action.performed -= OnToggleDebug;
            debugToggleAction.action.Disable();
        }

        if (addResource != null) 
            addResource.onClick.RemoveListener(OnClick_AddTenToAllResources);
    }

    private void InitializeUI()
    {
        if (resourceGridParent == null || resourceItemPrefab == null) return;

        foreach (Transform child in resourceGridParent) 
            Pool.Destroy(child.gameObject);

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            UIDebugPanel itemInstance = Instantiate(resourceItemPrefab, resourceGridParent);
            int currentAmount = playerResources != null ? playerResources.GetResource(type) : 0;
            
            itemInstance.Setup(type, currentAmount);
            uiItemMap[type] = itemInstance;
        }
    }

    private void HandleResourceChanged(ResourceType type, int newAmount)
    {
        if (uiItemMap.TryGetValue(type, out UIDebugPanel uiItem)) 
            uiItem.UpdateAmount(newAmount);
    }

    private void OnToggleDebug(InputAction.CallbackContext context)
    {
        if (debugButtonsPanel != null)
        {
            bool isCurrentlyActive = debugButtonsPanel.activeSelf;
            bool willBeActive = !isCurrentlyActive;

            debugButtonsPanel.SetActive(willBeActive);

            // Toggle cursor state alongside UI visibility
            Cursor.visible = willBeActive;
            Cursor.lockState = willBeActive ? CursorLockMode.None : CursorLockMode.Locked;

            Debug.Log($"Debug UI toggled: {willBeActive}");
        }
    }

    // --- Button Callbacks ---

    public void OnClick_AddTenToAllResources()
    {
        if (playerResources != null) 
            playerResources.AddTenToAllResources();
    }
}