using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiTrade : UiModals
{
    [Header("Left Side - Deposit Settings")]
    [SerializeField] private ResourceType depositResourceType = ResourceType.Wood;
    [SerializeField] private TMP_Text txtDepositResourceName;
    [SerializeField] private Button btnMinus;
    [SerializeField] private Button btnPlus;
    [SerializeField] private TMP_Text txtDepositAmount;

    [Header("Right Side - Receive Settings (Read-Only)")]
    [SerializeField] private ResourceType receiveResourceType = ResourceType.Iron;
    [SerializeField] private TMP_Text txtReceiveResourceName;
    [SerializeField] private TMP_Text txtReceiveAmount;

    [Header("Trade Action")]
    [SerializeField] private Button btnTrade;

    [Header("Conversion Settings")]
    [Tooltip("How many deposit resources are needed for 1 receive resource (e.g., 2 Wood = 1 Iron)")]
    [SerializeField] private int depositCostPerReceiveUnit = 2;

    private int currentDepositAmount = 0;
    private PlayerResources playerResources;

    private void Awake()
    {
        // Setup button listeners
        if (btnMinus != null) btnMinus.onClick.AddListener(OnMinusClicked);
        if (btnPlus != null) btnPlus.onClick.AddListener(OnPlusClicked);
        if (btnTrade != null) btnTrade.onClick.AddListener(ExecuteTrade);
    }

    protected override void Start()
    {
        base.Start();
        
        // Cache reference once
        if (GameManager.Instance != null && GameManager.Instance.playerController != null) 
            playerResources = GameManager.Instance.playerController.PlayerResources;

        if (playerResources == null)
        {
            Debug.LogError("UiTrade: PlayerResources component could not be found!");
        }

        // Initialize labels
        if (txtDepositResourceName != null) txtDepositResourceName.text = depositResourceType.ToString();
        if (txtReceiveResourceName != null) txtReceiveResourceName.text = receiveResourceType.ToString();

        UpdateDisplay();
    }

    private void OnMinusClicked()
    {
        // Lower trade amount by step of cost ratio (or 1)
        currentDepositAmount = Mathf.Max(0, currentDepositAmount - depositCostPerReceiveUnit);
        UpdateDisplay();
    }

    private void OnPlusClicked()
    {
        if (playerResources == null) return;

        int availablePlayerAmount = playerResources.GetResource(depositResourceType);

        // Cap maximum deposit amount to what the player actually owns
        if (currentDepositAmount + depositCostPerReceiveUnit <= availablePlayerAmount)
        {
            currentDepositAmount += depositCostPerReceiveUnit;
        }
        else
        {
            Debug.LogWarning($"Cannot offer more than owned {depositResourceType} ({availablePlayerAmount})");
        }

        UpdateDisplay();
    }

    private void ExecuteTrade()
    {
        if (playerResources == null) return;

        int calculatedReceiveAmount = CalculateReceiveAmount(currentDepositAmount);

        if (currentDepositAmount <= 0 || calculatedReceiveAmount <= 0)
        {
            Debug.LogWarning("Trade amount must be greater than zero!");
            return;
        }

        // Deduct traded resources and award new resources
        if (playerResources.SpendResource(depositResourceType, currentDepositAmount))
        {
            playerResources.AddResource(receiveResourceType, calculatedReceiveAmount);
            
            // Reset quantity display after successful trade
            currentDepositAmount = 0;
            UpdateDisplay();
        }
    }

    private int CalculateReceiveAmount(int depositAmount)
    {
        if (depositCostPerReceiveUnit <= 0) return 0;
        return depositAmount / depositCostPerReceiveUnit;
    }

    private void UpdateDisplay()
    {
        int receiveAmount = CalculateReceiveAmount(currentDepositAmount);

        if (txtDepositAmount != null) txtDepositAmount.text = currentDepositAmount.ToString();
        if (txtReceiveAmount != null) txtReceiveAmount.text = receiveAmount.ToString();

        // Enable trade button only if a valid conversion step is selected
        if (btnTrade != null)
        {
            btnTrade.interactable = currentDepositAmount > 0 && receiveAmount > 0;
        }
    }

    private void OnDestroy()
    {
        if (btnMinus != null) btnMinus.onClick.RemoveListener(OnMinusClicked);
        if (btnPlus != null) btnPlus.onClick.RemoveListener(OnPlusClicked);
        if (btnTrade != null) btnTrade.onClick.RemoveListener(ExecuteTrade);
    }
}