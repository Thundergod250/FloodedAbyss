using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiTrade : UiModals
{
    [Header("Trade Configuration")]
    [SerializeField] private TradeConfigSO tradeConfig;

    [Header("Left Side - Deposit Settings")]
    [SerializeField] private TMP_Dropdown dropdownDepositResource;
    [SerializeField] private Button btnMinusTen;  // "--" button
    [SerializeField] private Button btnMinusOne;  // "-" button
    [SerializeField] private Button btnPlusOne;   // "+" button
    [SerializeField] private Button btnPlusTen;   // "++" button
    [SerializeField] private TMP_Text txtDepositAmount;

    [Header("Right Side - Receive Settings")]
    [SerializeField] private TMP_Dropdown dropdownReceiveResource;
    [SerializeField] private TMP_Text txtReceiveAmount;

    [Header("Trade Action")]
    [SerializeField] private Button btnTrade;

    // Active dropdown options (excluding the opposite selected item)
    private List<ResourceType> activeDepositOptions = new();
    private List<ResourceType> activeReceiveOptions = new();

    private ResourceType selectedDepositType;
    private ResourceType selectedReceiveType;

    private int currentDepositAmount = 0;
    private PlayerResources playerResources;

    private void Awake()
    {
        // Bind step button listeners
        if (btnMinusTen != null) btnMinusTen.onClick.AddListener(() => AdjustDepositAmount(-10));
        if (btnMinusOne != null) btnMinusOne.onClick.AddListener(() => AdjustDepositAmount(-1));
        if (btnPlusOne != null) btnPlusOne.onClick.AddListener(() => AdjustDepositAmount(1));
        if (btnPlusTen != null) btnPlusTen.onClick.AddListener(() => AdjustDepositAmount(10));

        if (btnTrade != null) btnTrade.onClick.AddListener(ExecuteTrade);

        // Bind dropdown listeners
        if (dropdownDepositResource != null) dropdownDepositResource.onValueChanged.AddListener(OnDepositResourceChanged);
        if (dropdownReceiveResource != null) dropdownReceiveResource.onValueChanged.AddListener(OnReceiveResourceChanged);
    }

    protected override void Start()
    {
        base.Start();

        if (tradeConfig == null)
        {
            Debug.LogError("UiTrade: TradeConfigSO asset is missing! Please assign it in the Inspector.");
            return;
        }

        // Cache PlayerResources reference once
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            playerResources = GameManager.Instance.playerController.PlayerResources;

        if (playerResources == null)
        {
            Debug.LogError("UiTrade: PlayerResources component could not be found!");
        }

        // Set initial defaults from config
        var availableResources = tradeConfig.AvailableTradeResources;
        if (availableResources != null && availableResources.Count > 1)
        {
            selectedDepositType = availableResources[0];
            selectedReceiveType = availableResources[1];
        }

        RefreshDropdownOptions();
        UpdateDisplay();
    }

    private void RefreshDropdownOptions()
    {
        if (tradeConfig == null) return;

        dropdownDepositResource.onValueChanged.RemoveListener(OnDepositResourceChanged);
        dropdownReceiveResource.onValueChanged.RemoveListener(OnReceiveResourceChanged);

        var availableResources = tradeConfig.AvailableTradeResources;

        // 1. Build Left Options (Exclude right selection)
        activeDepositOptions.Clear();
        List<string> depositStrings = new List<string>();
        foreach (ResourceType type in availableResources)
        {
            if (type != selectedReceiveType)
            {
                activeDepositOptions.Add(type);
                depositStrings.Add(type.ToString());
            }
        }

        dropdownDepositResource.ClearOptions();
        dropdownDepositResource.AddOptions(depositStrings);
        dropdownDepositResource.value = Mathf.Max(0, activeDepositOptions.IndexOf(selectedDepositType));

        // 2. Build Right Options (Exclude left selection)
        activeReceiveOptions.Clear();
        List<string> receiveStrings = new List<string>();
        foreach (ResourceType type in availableResources)
        {
            if (type != selectedDepositType)
            {
                activeReceiveOptions.Add(type);
                receiveStrings.Add(type.ToString());
            }
        }

        dropdownReceiveResource.ClearOptions();
        dropdownReceiveResource.AddOptions(receiveStrings);
        dropdownReceiveResource.value = Mathf.Max(0, activeReceiveOptions.IndexOf(selectedReceiveType));

        dropdownDepositResource.onValueChanged.AddListener(OnDepositResourceChanged);
        dropdownReceiveResource.onValueChanged.AddListener(OnReceiveResourceChanged);
    }

    private void OnDepositResourceChanged(int index)
    {
        if (index >= 0 && index < activeDepositOptions.Count)
        {
            selectedDepositType = activeDepositOptions[index];
            currentDepositAmount = 0;
            RefreshDropdownOptions();
            UpdateDisplay();
        }
    }

    private void OnReceiveResourceChanged(int index)
    {
        if (index >= 0 && index < activeReceiveOptions.Count)
        {
            selectedReceiveType = activeReceiveOptions[index];
            currentDepositAmount = 0;
            RefreshDropdownOptions();
            UpdateDisplay();
        }
    }

    private void AdjustDepositAmount(int delta)
    {
        if (delta > 0)
        {
            if (playerResources == null) return;

            int availablePlayerAmount = playerResources.GetResource(selectedDepositType);
            int maxAddable = availablePlayerAmount - currentDepositAmount;

            if (maxAddable <= 0)
            {
                Debug.LogWarning($"Cannot offer more than owned {selectedDepositType} ({availablePlayerAmount})");
                return;
            }

            currentDepositAmount += Mathf.Min(delta, maxAddable);
        }
        else if (delta < 0)
        {
            currentDepositAmount = Mathf.Max(0, currentDepositAmount + delta);
        }

        UpdateDisplay();
    }

    private void ExecuteTrade()
    {
        if (playerResources == null || tradeConfig == null) return;

        int calculatedReceiveAmount = tradeConfig.CalculateReceiveAmount(selectedDepositType, selectedReceiveType, currentDepositAmount);

        if (currentDepositAmount <= 0 || calculatedReceiveAmount <= 0)
        {
            Debug.LogWarning("Trade amount must be greater than zero!");
            return;
        }

        // Spend input resource and add output resource
        if (playerResources.SpendResource(selectedDepositType, currentDepositAmount))
        {
            playerResources.AddResource(selectedReceiveType, calculatedReceiveAmount);

            currentDepositAmount = 0;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        int receiveAmount = tradeConfig != null
            ? tradeConfig.CalculateReceiveAmount(selectedDepositType, selectedReceiveType, currentDepositAmount)
            : 0;

        if (txtDepositAmount != null) txtDepositAmount.text = currentDepositAmount.ToString();
        if (txtReceiveAmount != null) txtReceiveAmount.text = receiveAmount.ToString();

        if (btnTrade != null)
        {
            btnTrade.interactable = currentDepositAmount > 0 && receiveAmount > 0;
        }
    }

    private void OnDestroy()
    {
        if (btnMinusTen != null) btnMinusTen.onClick.RemoveAllListeners();
        if (btnMinusOne != null) btnMinusOne.onClick.RemoveAllListeners();
        if (btnPlusOne != null) btnPlusOne.onClick.RemoveAllListeners();
        if (btnPlusTen != null) btnPlusTen.onClick.RemoveAllListeners();
        if (btnTrade != null) btnTrade.onClick.RemoveAllListeners();

        if (dropdownDepositResource != null) dropdownDepositResource.onValueChanged.RemoveAllListeners();
        if (dropdownReceiveResource != null) dropdownReceiveResource.onValueChanged.RemoveAllListeners();
    }
}
