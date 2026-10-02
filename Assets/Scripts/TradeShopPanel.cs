using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeShopPanel : UiModals
{
    [System.Serializable]
    public class TradeColumn
    {
        [Header("Resource Types")]
        public ResourceType targetResource; // Resource to GAIN (e.g., Wood)
        public ResourceType costResource;   // Resource to SPEND (e.g., Stone)
        public int yieldPerTrade = 5;       // Yield ratio (e.g., 5 Wood)
        public int costPerTrade = 1;        // Cost ratio (e.g., 1 Stone)

        [Header("UI Controls")]
        public Button incrementButton; // '>' Button to add to trade count
        public Button decrementButton; // '<' Button to lower trade count
        public Button executeButton;   // '>>' or 'Trade' Button to perform the trade
        public TextMeshProUGUI tradeAmountText; // Text displaying current selected trade count (e.g. "1")
        public TextMeshProUGUI resourceDisplayText; // Text showing total player inventory count

        [HideInInspector] public int selectedTradeCount = 1;
    }

    [Header("Trade Columns")]
    public TradeColumn woodTradeColumn;   // Trade Stone -> Wood
    public TradeColumn stoneTradeColumn;  // Trade Copper -> Stone
    public TradeColumn copperTradeColumn; // Trade Iron -> Copper
    public TradeColumn ironTradeColumn;   // Trade Gold -> Iron

    private PlayerResources playerResources;

    protected void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }

        // Initialize Column Setups & Listeners
        SetupColumn(woodTradeColumn, ResourceType.Wood, ResourceType.Stone);
        SetupColumn(stoneTradeColumn, ResourceType.Stone, ResourceType.Copper);
        SetupColumn(copperTradeColumn, ResourceType.Copper, ResourceType.Iron);
        SetupColumn(ironTradeColumn, ResourceType.Iron, ResourceType.Gold);

        UpdateUI();
    }

    private void SetupColumn(TradeColumn col, ResourceType target, ResourceType cost)
    {
        if (col == null) return;

        col.targetResource = target;
        col.costResource = cost;
        col.selectedTradeCount = 1;

        if (col.incrementButton != null)
            col.incrementButton.onClick.AddListener(() => AdjustTradeAmount(col, 1));

        if (col.decrementButton != null)
            col.decrementButton.onClick.AddListener(() => AdjustTradeAmount(col, -1));

        if (col.executeButton != null)
            col.executeButton.onClick.AddListener(() => ExecuteTrade(col));
    }

    private void Update()
    {
        UpdateUI();
    }

    /// <summary> Increments or decrements the staged trade count for a column </summary>
    public void AdjustTradeAmount(TradeColumn col, int delta)
    {
        if (col == null || playerResources == null) return;

        int availableCostResource = playerResources.GetResource(col.costResource);
        int maxPossibleTrades = Mathf.Max(1, availableCostResource / col.costPerTrade);

        // Clamp staged amount between 1 and the max affordable amount
        col.selectedTradeCount = Mathf.Clamp(col.selectedTradeCount + delta, 1, maxPossibleTrades);
        UpdateUI();
    }

    /// <summary> Commits the staged trade amount </summary>
    public void ExecuteTrade(TradeColumn col)
    {
        if (col == null || playerResources == null) return;

        int totalCost = col.selectedTradeCount * col.costPerTrade;
        int totalYield = col.selectedTradeCount * col.yieldPerTrade;

        if (playerResources.GetResource(col.costResource) >= totalCost)
        {
            playerResources.AddResource(col.costResource, -totalCost);
            playerResources.AddResource(col.targetResource, totalYield);

            Debug.Log($"[TradeShop] Traded {totalCost} {col.costResource} for {totalYield} {col.targetResource}!");

            // Reset staged amount back to 1 after successful trade
            col.selectedTradeCount = 1;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning($"[TradeShop] Not enough {col.costResource} to perform this trade!");
        }
    }

    private void UpdateUI()
    {
        UpdateColumnUI(woodTradeColumn);
        UpdateColumnUI(stoneTradeColumn);
        UpdateColumnUI(copperTradeColumn);
        UpdateColumnUI(ironTradeColumn);
    }

    private void UpdateColumnUI(TradeColumn col)
    {
        if (col == null || playerResources == null) return;

        // Display current inventory total
        if (col.resourceDisplayText != null)
        {
            col.resourceDisplayText.text = playerResources.GetResource(col.targetResource).ToString();
        }

        // Display selected staged trade amount
        if (col.tradeAmountText != null)
        {
            col.tradeAmountText.text = col.selectedTradeCount.ToString();
        }

        int currentCostResource = playerResources.GetResource(col.costResource);
        int totalCost = col.selectedTradeCount * col.costPerTrade;

        // Enable/Disable buttons dynamically
        if (col.executeButton != null)
            col.executeButton.interactable = currentCostResource >= totalCost;

        if (col.decrementButton != null)
            col.decrementButton.interactable = col.selectedTradeCount > 1;

        if (col.incrementButton != null)
            col.incrementButton.interactable = currentCostResource >= (col.selectedTradeCount + 1) * col.costPerTrade;
    }
}