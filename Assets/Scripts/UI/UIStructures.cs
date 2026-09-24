using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStructures : UiModals
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text resourceNameText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text extraStatusText;
    [SerializeField] private Button increaseButton;   // '>' button
    [SerializeField] private Button decreaseButton;  // '<' button

    [Header("Recruitment UI (For StructureHouse)")]
    [SerializeField] private GameObject recruitmentContainer; 
    [SerializeField] private Button recruitButton;
    [SerializeField] private TMP_Text recruitmentCostText;

    [Header("Settings")]
    [SerializeField] private int transferStepAmount = 1;

    private ItemStructure currentStructure;
    public ItemStructure CurrentStructure => currentStructure;

    protected override void Initialize()
    {
        base.Initialize();

        if (increaseButton != null)
            increaseButton.onClick.AddListener(OnDepositClicked);

        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(OnWithdrawClicked);

        if (recruitButton != null)
            recruitButton.onClick.AddListener(OnRecruitClicked);
    }

    public void SetupStructure(ItemStructure structure)
    {
        currentStructure = structure;
        UpdateUI();
    }

    private void OnDepositClicked()
    {
        if (currentStructure != null && currentStructure.DepositResource(transferStepAmount))
        {
            UpdateUI();
        }
    }

    private void OnWithdrawClicked()
    {
        if (currentStructure != null && currentStructure.WithdrawResource(transferStepAmount))
        {
            UpdateUI();
        }
    }

    private void OnRecruitClicked()
    {
        if (currentStructure is StructureHouse house)
        {
            if (house.TryRecruitCitizen())
            {
                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        if (currentStructure == null) return;

        var res = currentStructure.TargetResource;

        // Custom label when inspecting a house
        if (resourceNameText != null)
            resourceNameText.text = currentStructure is StructureHouse ? "Housed Citizens" : res.resourceType.ToString();

        if (capacityText != null)
            capacityText.text = $"{res.currentAmount} / {res.maxCapacity}";

        // 1. Generator Status Handling
        if (extraStatusText != null)
        {
            if (currentStructure is StructureGenerator)
            {
                extraStatusText.gameObject.SetActive(true);
                int currentEnergy = 0;
                if (GameManager.Instance != null && GameManager.Instance.playerController != null && GameManager.Instance.playerController.PlayerResources != null)
                {
                    currentEnergy = GameManager.Instance.playerController.PlayerResources.GetResource(ResourceType.Energy);
                }
                extraStatusText.text = $"Energy: {currentEnergy}";
            }
            else
            {
                extraStatusText.gameObject.SetActive(false);
            }
        }

        // 2. House Recruitment Handling
        if (recruitmentContainer != null)
        {
            if (currentStructure is StructureHouse house)
            {
                recruitmentContainer.SetActive(true);

                // Format cost requirements text
                if (recruitmentCostText != null)
                {
                    string costStr = "Cost: ";
                    foreach (var cost in house.RecruitmentCost)
                    {
                        costStr += $"{cost.amount} {cost.resourceType} ";
                    }
                    recruitmentCostText.text = costStr;
                }

                // Check player resources to enable/disable recruit button
                if (recruitButton != null && GameManager.Instance != null && GameManager.Instance.playerController != null)
                {
                    var playerRes = GameManager.Instance.playerController.PlayerResources;
                    recruitButton.interactable = playerRes != null && playerRes.CanAfford(house.RecruitmentCost);
                }
            }
            else
            {
                recruitmentContainer.SetActive(false);
            }
        }

        // 3. Arrow Buttons interactability
        if (decreaseButton != null)
            decreaseButton.interactable = currentStructure.CanWithdraw && res.currentAmount > 0;

        if (increaseButton != null)
            increaseButton.interactable = res.currentAmount < res.maxCapacity;
    }

    private void OnCloseClicked()
    {
        GameManager.Instance.uiController.CloseAllModals();
    }
}