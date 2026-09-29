using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShop : UiModals
{
    [Header("UI Buttons")]
    [SerializeField] private Button arrowLeftButton;
    [SerializeField] private Button arrowRightButton;
    [SerializeField] private Button tradeButton;

    [Header("UI Displays")]
    [SerializeField] private TMP_Text depositTextDisplay;      // Displays resource to deposit & total cost
    [SerializeField] private TMP_Text getContainerTextDisplay; // Displays resource to get & total yield
    [SerializeField] private TMP_Text tradeAmountText;          // Center circle text showing quantity multiplier

    [Header("Quantity Settings")]
    [SerializeField] private int minQuantity = 1;
    [SerializeField] private int quantityStep = 1;

    private ShopNPC activeShopNPC;
    private int currentQuantity = 1;
    private PlayerResources playerResources;

    protected override void OnEnable()
    {
        base.OnEnable();
        SetupButtonListeners();
        RefreshTradeUI();
    }

    public override void SetModalActive(bool active)
    {
        base.SetModalActive(active);
        if (active)
        {
            SetupButtonListeners();
            RefreshTradeUI();
        }
    }

    /// <summary>
    /// Passes the active ShopNPC configuration into the shop UI.
    /// </summary>
    public void OpenTradeShop(ShopNPC shopNPC)
    {
        activeShopNPC = shopNPC;
        currentQuantity = minQuantity;

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Shop);
        }

        RefreshTradeUI();
    }

    private void SetupButtonListeners()
    {
        if (arrowLeftButton != null)
        {
            arrowLeftButton.onClick.RemoveAllListeners();
            arrowLeftButton.onClick.AddListener(LeftArrowFunction);
        }

        if (arrowRightButton != null)
        {
            arrowRightButton.onClick.RemoveAllListeners();
            arrowRightButton.onClick.AddListener(RightArrowFunction);
        }

        if (tradeButton != null)
        {
            tradeButton.onClick.RemoveAllListeners();
            tradeButton.onClick.AddListener(Trade);
        }
    }

    // --- Arrow Functions (Quantity Adjustments) ---

    public void LeftArrowFunction()
    {
        currentQuantity = Mathf.Max(minQuantity, currentQuantity - quantityStep);
        RefreshTradeUI();
    }

    public void RightArrowFunction()
    {
        FindPlayerResources();
        if (activeShopNPC == null || playerResources == null) return;

        int ownedAmount = playerResources.GetResource(activeShopNPC.DepositResource);
        int maxAffordableQuantity = Mathf.Max(minQuantity, ownedAmount / Mathf.Max(1, activeShopNPC.DepositCostPerUnit));

        currentQuantity = Mathf.Min(maxAffordableQuantity, currentQuantity + quantityStep);
        RefreshTradeUI();
    }

    // --- Trade Execution ---

    public void Trade()
    {
        FindPlayerResources();
        if (activeShopNPC == null || playerResources == null) return;

        int totalDepositCost = activeShopNPC.DepositCostPerUnit * currentQuantity;
        int totalGetYield = activeShopNPC.GetYieldPerUnit * currentQuantity;

        if (playerResources.SpendResource(activeShopNPC.DepositResource, totalDepositCost))
        {
            playerResources.AddResource(activeShopNPC.GetResource, totalGetYield);
            Debug.Log($"[UIShop] Successfully traded {totalDepositCost} {activeShopNPC.DepositResource} for {totalGetYield} {activeShopNPC.GetResource}!");

            currentQuantity = minQuantity;
            RefreshTradeUI();
        }
        else
        {
            Debug.LogWarning($"[UIShop] Not enough {activeShopNPC.DepositResource} to complete trade!");
        }
    }

    // --- UI Update Helper ---

    public void RefreshTradeUI()
    {
        FindPlayerResources();

        if (activeShopNPC == null)
        {
            if (depositTextDisplay != null) depositTextDisplay.text = "No Shop NPC";
            if (getContainerTextDisplay != null) getContainerTextDisplay.text = "No Shop NPC";
            if (tradeAmountText != null) tradeAmountText.text = "0";
            if (tradeButton != null) tradeButton.interactable = false;
            return;
        }

        int totalDepositCost = activeShopNPC.DepositCostPerUnit * currentQuantity;
        int totalGetYield = activeShopNPC.GetYieldPerUnit * currentQuantity;
        int ownedDeposit = playerResources != null ? playerResources.GetResource(activeShopNPC.DepositResource) : 0;

        // Update Left Container (Deposit Info)
        if (depositTextDisplay != null)
        {
            depositTextDisplay.text = $"DEPOSIT\n{activeShopNPC.DepositResource}\nx{totalDepositCost}\n(Owned: {ownedDeposit})";
        }

        // Update Right Container (Get Info)
        if (getContainerTextDisplay != null)
        {
            getContainerTextDisplay.text = $"RECEIVE\n{activeShopNPC.GetResource}\nx{totalGetYield}";
        }

        // Update Center Quantity Display
        if (tradeAmountText != null)
        {
            tradeAmountText.text = currentQuantity.ToString();
        }

        // Validate Trade & Arrow Buttons
        bool canAfford = playerResources != null && ownedDeposit >= totalDepositCost && totalDepositCost > 0;

        if (tradeButton != null)
        {
            tradeButton.interactable = canAfford;
        }

        if (arrowLeftButton != null)
        {
            arrowLeftButton.interactable = currentQuantity > minQuantity;
        }

        if (arrowRightButton != null)
        {
            arrowRightButton.interactable = ownedDeposit >= (activeShopNPC.DepositCostPerUnit * (currentQuantity + quantityStep));
        }
    }

    private void FindPlayerResources()
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