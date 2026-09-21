using System.Collections;
using UnityEngine;
using TMPro;

public class UIShop : UiModals
{
    public enum UpgradeType
    {
        PickaxeAttack,
        PickaxeSpeed,
        PlayerStamina,
        PlayerOxygen
    }

    [System.Serializable]
    public class UpgradeConfig
    {
        public UpgradeType upgradeType;
        public string title;
        public Sprite icon;
        [TextArea(2, 3)] public string description;

        [Header("Cost Settings")]
        public ResourceType costResourceType = ResourceType.Stone;
        public int baseCost = 10;
        public int costIncreasePerLevel = 10;

        [Header("Stat Increase Settings")]
        public float valueIncreasePerLevel = 5f;
        public int maxLevel = 10;

        [Header("UI Card Assignment")]
        public UIShopCard shopCardUI;

        [HideInInspector] public int currentLevel = 0;

        public int GetCurrentCost() => baseCost + (currentLevel * costIncreasePerLevel);
        public bool IsMaxLevel => currentLevel >= maxLevel;
    }

    [Header("Upgrade Configurations (4 Items)")]
    [SerializeField] private UpgradeConfig pickaxeAttackUpgrade;
    [SerializeField] private UpgradeConfig pickaxeSpeedUpgrade;
    [SerializeField] private UpgradeConfig playerStaminaUpgrade;
    [SerializeField] private UpgradeConfig playerOxygenUpgrade;

    private PlayerResources playerResources;

    protected override void OnEnable()
    {
        base.OnEnable();
        RefreshShopUI();
    }

    public override void SetModalActive(bool active)
    {
        base.SetModalActive(active);
        if (active)
        {
            RefreshShopUI();
        }
    }

    public void RefreshShopUI()
    {
        FindDependencies();

        SetupUpgradeCard(pickaxeAttackUpgrade);
        SetupUpgradeCard(pickaxeSpeedUpgrade);
        SetupUpgradeCard(playerStaminaUpgrade);
        SetupUpgradeCard(playerOxygenUpgrade);
    }

    private void SetupUpgradeCard(UpgradeConfig config)
    {
        if (config == null || config.shopCardUI == null) return;

        int currentCost = config.GetCurrentCost();
        bool canAfford = CheckCanAfford(config.costResourceType, currentCost);

        string levelInfo = config.IsMaxLevel ? "MAX LEVEL" : $"Lvl {config.currentLevel} / {config.maxLevel}";
        string costInfo = config.IsMaxLevel ? "MAX" : $"{config.costResourceType}: {currentCost}";

        config.shopCardUI.SetupCard(
            config.title,
            config.icon,
            config.description,
            levelInfo,
            costInfo,
            canAfford,
            config.IsMaxLevel,
            () => PurchaseUpgrade(config)
        );
    }

    private bool CheckCanAfford(ResourceType resourceType, int amount)
    {
        if (playerResources == null) return false;

        // Uses GetResource from PlayerResources to check balance
        return playerResources.GetResource(resourceType) >= amount;
    }

    private void PurchaseUpgrade(UpgradeConfig config)
    {
        if (config == null || config.IsMaxLevel) return;

        FindDependencies();

        int cost = config.GetCurrentCost();

        // Uses SpendResource from PlayerResources to perform transaction
        if (playerResources != null && playerResources.SpendResource(config.costResourceType, cost))
        {
            config.currentLevel++;
            ApplyUpgradeEffect(config);
            RefreshShopUI();
            Debug.Log($"[UIShop] Purchased {config.title} (Level {config.currentLevel})");
        }
        else
        {
            Debug.LogWarning($"[UIShop] Cannot afford {config.title}!");
        }
    }

    private void ApplyUpgradeEffect(UpgradeConfig config)
    {
        if (GameManager.Instance == null || GameManager.Instance.playerController == null) return;

        GameObject player = GameManager.Instance.playerController.gameObject;

        switch (config.upgradeType)
        {
            case UpgradeType.PickaxeAttack:
                TemporaryAttack attackComp = player.GetComponent<TemporaryAttack>();
                if (attackComp != null)
                {
                    attackComp.AddDamage(Mathf.RoundToInt(config.valueIncreasePerLevel));
                }
                break;

            case UpgradeType.PickaxeSpeed:
                Debug.Log($"[UIShop] Increased Pickaxe Swing Speed by {config.valueIncreasePerLevel}%");
                break;

            case UpgradeType.PlayerStamina:
                Debug.Log($"[UIShop] Increased Max Stamina by {config.valueIncreasePerLevel}");
                break;

            case UpgradeType.PlayerOxygen:
                Debug.Log($"[UIShop] Increased Max Oxygen by {config.valueIncreasePerLevel}");
                break;
        }
    }

    private void FindDependencies()
    {
        if (playerResources == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                playerResources = GameManager.Instance.playerController.GetComponent<PlayerResources>();
            }
            else
            {
                playerResources = FindAnyObjectByType<PlayerResources>();
            }
        }
    }

    public void CloseShop()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }
}