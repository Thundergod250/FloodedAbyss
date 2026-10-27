using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("FPS Counter Settings")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float fpsUpdateInterval = 0.5f;

    [Header("Input Action")]
    [SerializeField] private InputActionReference debugToggleAction;

    [Header("Debug Buttons")]
    [SerializeField] private Button addResource;
    [SerializeField] private Button upgradeStats;

    private PlayerResources playerResources;
    private PlayerStats playerStats;
    private Dictionary<ResourceType, UIDebugPanel> uiItemMap = new();
    private Dictionary<string, UIDebugPanel> uiStatMap = new();

    private float frameAccumulator = 0f;
    private int frameCount = 0;
    private float fpsTimeLeft = 0f;

    private void Start()
    {
        BindPlayerReferences();
        InitializeUI();
        RefreshStatsUI();
        fpsTimeLeft = fpsUpdateInterval;
    }

    private void Update()
    {
        UpdateFPSCounter();
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

    private void UpdateFPSCounter()
    {
        if (fpsText == null) return;

        fpsTimeLeft -= Time.unscaledDeltaTime;
        frameAccumulator += Time.unscaledDeltaTime / Time.timeScale;
        frameCount++;

        if (fpsTimeLeft <= 0.0f)
        {
            float fps = frameCount / frameAccumulator;
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";

            fpsTimeLeft = fpsUpdateInterval;
            frameAccumulator = 0.0f;
            frameCount = 0;
        }
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

            // Uses CursorManager with the boolean parameter
            if (GameManager.Instance != null && GameManager.Instance.CursorManager != null)
            {
                GameManager.Instance.CursorManager.SetCursorVisibility(willBeActive);
            }
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
            playerStats.UpgradeAllStats();
    }
}