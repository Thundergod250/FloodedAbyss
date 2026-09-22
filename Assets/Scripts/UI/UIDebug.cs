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
    [SerializeField] private Button upgradeStats;

    private PlayerResources playerResources;
    private PlayerStats playerStats;
    private Dictionary<ResourceType, UIDebugPanel> uiItemMap = new();
    private Dictionary<string, UIDebugPanel> uiStatMap = new();

    private void Start()
    {
        // Fetch references and setup event listeners if not already done in OnEnable
        BindPlayerReferences();

        InitializeUI();
        InitializeStatsUI();
        RefreshStatsUI(); // Ensure fresh state on startup
    }

    private void OnEnable()
    {
        BindPlayerReferences();

        if (debugToggleAction != null)
        {
            debugToggleAction.action.Enable();
            debugToggleAction.action.performed += OnToggleDebug;
        }

        if (addResource != null) 
            addResource.onClick.AddListener(OnClick_AddTenToAllResources);

        if (upgradeStats != null)
            upgradeStats.onClick.AddListener(OnClick_UpgradeAllStats);
    }

    private void OnDisable()
    {
        UnbindPlayerReferences();

        if (debugToggleAction != null)
        {
            debugToggleAction.action.performed -= OnToggleDebug;
            debugToggleAction.action.Disable();
        }

        if (addResource != null) 
            addResource.onClick.RemoveListener(OnClick_AddTenToAllResources);

        if (upgradeStats != null)
            upgradeStats.onClick.RemoveListener(OnClick_UpgradeAllStats);
    }

    private void BindPlayerReferences()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            if (playerResources == null)
            {
                playerResources = GameManager.Instance.playerController.PlayerResources;
                if (playerResources != null)
                    playerResources.EvtOnResourceChanged.AddListener(HandleResourceChanged);
            }

            if (playerStats == null)
            {
                playerStats = GameManager.Instance.playerController.PlayerStats;
                if (playerStats != null)
                    playerStats.EvtOnStatChanged.AddListener(RefreshStatsUI);
            }
        }
    }

    private void UnbindPlayerReferences()
    {
        if (playerResources != null)
        {
            playerResources.EvtOnResourceChanged.RemoveListener(HandleResourceChanged);
            playerResources = null;
        }

        if (playerStats != null)
        {
            playerStats.EvtOnStatChanged.RemoveListener(RefreshStatsUI);
            playerStats = null;
        }
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
        }
    }
}