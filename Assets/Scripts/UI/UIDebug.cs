using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIDebug : MonoBehaviour
{
    [Header("UI Dynamic Layout")]
    [SerializeField] private GameObject debugButtonsPanel;
    [SerializeField] private Transform resourceGridParent;
    [SerializeField] private UIDebugPanel resourceItemPrefab;

    [Header("Player Stats Layout")]
    [SerializeField] private Transform statsGridParent;
    [SerializeField] private UIDebugPanel statItemPrefab;

    [Header("Input Action")]
    [SerializeField] private InputActionReference debugToggleAction;

    [Header("Debug Buttons")]
    [SerializeField] private Button addResource;
    [SerializeField] private Button upgradeStats; // <-- Added button reference

    private PlayerResources playerResources;
    private PlayerStats playerStats;
    private Dictionary<ResourceType, UIDebugPanel> uiItemMap = new();
    private Dictionary<string, UIDebugPanel> uiStatMap = new();

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
            playerStats = GameManager.Instance.playerController.PlayerStats;
        }

        if (playerResources != null)
            playerResources.EvtOnResourceChanged.AddListener(HandleResourceChanged);

        InitializeUI();
        InitializeStatsUI();
    }

    private void OnEnable()
    {
        // Re-subscribe if playerResources was already assigned in Start
        if (playerResources != null) 
            playerResources.EvtOnResourceChanged.AddListener(HandleResourceChanged);

        if (debugToggleAction != null)
        {
            debugToggleAction.action.Enable();
            debugToggleAction.action.performed += OnToggleDebug;
        }

        if (addResource != null) 
            addResource.onClick.AddListener(OnClick_AddTenToAllResources);

        if (upgradeStats != null)
            upgradeStats.onClick.AddListener(OnClick_UpgradeAllStats); // <-- Subscribe button listener
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

        if (upgradeStats != null)
            upgradeStats.onClick.RemoveListener(OnClick_UpgradeAllStats); // <-- Unsubscribe button listener
    }

    private void InitializeUI()
    {
        if (resourceGridParent == null || resourceItemPrefab == null) return;

        foreach (Transform child in resourceGridParent) 
            Pool.Destroy(child.gameObject);

        uiItemMap.Clear();

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            UIDebugPanel itemInstance = Instantiate(resourceItemPrefab, resourceGridParent);
            int currentAmount = playerResources != null ? playerResources.GetResource(type) : 0;
            
            itemInstance.SetupResource(type, currentAmount);
            uiItemMap[type] = itemInstance;
        }
    }

    private void InitializeStatsUI()
    {
        if (statsGridParent == null || statItemPrefab == null || playerStats == null) return;

        foreach (Transform child in statsGridParent)
            Pool.Destroy(child.gameObject);

        uiStatMap.Clear();

        // Create initial rows for 4-column stats
        CreateStatRow("Health", "Player", "Max HP");
        CreateStatRow("Stamina", "Player", "Max Stamina");
        CreateStatRow("Oxygen", "Player", "Max Oxygen");
        CreateStatRow("AxeDamage", "Axe", "Damage");
        CreateStatRow("AxeSpeed", "Axe", "Attack Speed");
    }

    private void CreateStatRow(string statKey, string category, string statType)
    {
        UIDebugPanel itemInstance = Instantiate(statItemPrefab, statsGridParent);
        int lvl = playerStats.GetStatLevel(statKey);
        string val = playerStats.GetStatValueString(statKey);

        itemInstance.SetupStat(category, statType, lvl, val);
        uiStatMap[statKey] = itemInstance;
    }

    public void RefreshStatsUI()
    {
        if (playerStats == null) return;

        foreach (var pair in uiStatMap)
        {
            int lvl = playerStats.GetStatLevel(pair.Key);
            string val = playerStats.GetStatValueString(pair.Key);
            pair.Value.UpdateStat(lvl, val);
        }
    }

    private void HandleResourceChanged(ResourceType type, int newAmount)
    {
        if (uiItemMap.TryGetValue(type, out UIDebugPanel uiItem)) 
            uiItem.UpdateResourceAmount(newAmount);
    }

    private void OnToggleDebug(InputAction.CallbackContext context)
    {
        if (debugButtonsPanel != null)
        {
            bool isCurrentlyActive = debugButtonsPanel.activeSelf;
            bool willBeActive = !isCurrentlyActive;

            debugButtonsPanel.SetActive(willBeActive);

            if (willBeActive)
                RefreshStatsUI();

            Cursor.visible = willBeActive;
            Cursor.lockState = willBeActive ? CursorLockMode.None : CursorLockMode.Locked;

            Debug.Log($"Debug UI toggled: {willBeActive}");
        }
    }

    public void OnClick_AddTenToAllResources()
    {
        if (playerResources != null) 
            playerResources.AddTenToAllResources();
    }

    public void OnClick_UpgradeAllStats()
    {
        if (playerStats != null)
        {
            playerStats.UpgradeAllStats();
            RefreshStatsUI(); // Immediately refresh stat values in UI
        }
    }
}