using System.Collections.Generic;
using UnityEngine;

public class UiUpgrade : UiModals
{
    [Header("Configuration")]
    [SerializeField] private UpgradeConfigSO upgradeConfig;

    [Header("UI Prefabs & Containers")]
    [SerializeField] private Transform rowContainer; 
    [SerializeField] private UiUpgradeRow rowPrefab;     

    private Dictionary<UpgradeType, int> upgradeLevels = new();
    private PlayerResources playerResources;

    protected override void Start()
    {
        base.Start();

        // Initialize default upgrade levels to Level 1
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            if (!upgradeLevels.ContainsKey(type))
                upgradeLevels[type] = 1;
        }

        TryBindPlayerResources();
        RefreshUI();
    }

    private void OnEnable()
    {
        TryBindPlayerResources();
        if (playerResources != null)
        {
            playerResources.EvtOnResourceChanged.AddListener(OnResourceChanged);
        }
        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerResources != null)
        {
            playerResources.EvtOnResourceChanged.RemoveListener(OnResourceChanged);
        }
    }

    private void TryBindPlayerResources()
    {
        if (playerResources == null && GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
            if (playerResources != null)
            {
                playerResources.EvtOnResourceChanged.RemoveListener(OnResourceChanged);
                playerResources.EvtOnResourceChanged.AddListener(OnResourceChanged);
            }
        }
    }

    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        // Refresh UI state whenever any resource count changes
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (upgradeConfig == null || rowPrefab == null || rowContainer == null) return;

        TryBindPlayerResources();

        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in upgradeConfig.Upgrades)
        {
            int currentLvl = upgradeLevels[data.type];
            int cost = upgradeConfig.GetCostForLevel(data.type, currentLvl);

            int currentOwned = playerResources != null ? playerResources.GetResource(data.costResource) : 0;
            bool canAfford = currentOwned >= cost && currentLvl < data.maxLevel;

            UiUpgradeRow newRow = Instantiate(rowPrefab, rowContainer);
            newRow.SetupRow(
                data.icon, 
                data.title, 
                currentLvl,
                data.maxLevel,
                cost, 
                data.costResource.ToString(),
                canAfford, 
                () => TryPurchaseUpgrade(data.type)
            );
        }
    }

    private void TryPurchaseUpgrade(UpgradeType type)
    {
        TryBindPlayerResources();
        if (playerResources == null || upgradeConfig == null) return;

        var data = upgradeConfig.GetUpgradeData(type);
        int currentLvl = upgradeLevels[type];
        int cost = upgradeConfig.GetCostForLevel(type, currentLvl);

        if (currentLvl >= data.maxLevel) return;

        // Spends resource and auto-triggers EvtOnResourceChanged inside PlayerResources
        if (playerResources.SpendResource(data.costResource, cost))
        {
            upgradeLevels[type]++;
            RefreshUI();
        }
    }
}