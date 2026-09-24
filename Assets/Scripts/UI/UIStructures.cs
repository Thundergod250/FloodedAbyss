using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStructures : UiModals
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text resourceNameText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private Button increaseButton;   // '>' button
    [SerializeField] private Button decreaseButton;  // '<' button

    [Header("Settings")]
    [SerializeField] private int transferStepAmount = 1; // Amount transferred per click

    private ItemStructure currentStructure;

    protected override void Initialize()
    {
        base.Initialize();

        if (increaseButton != null)
            increaseButton.onClick.AddListener(OnDepositClicked);

        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(OnWithdrawClicked);
    }

    public void SetupStructure(ItemStructure structure)
    {
        currentStructure = structure;
        UpdateUI();
    }

    private void OnDepositClicked()
    {
        if (currentStructure != null)
        {
            if (currentStructure.DepositResource(transferStepAmount))
            {
                UpdateUI();
            }
        }
    }

    private void OnWithdrawClicked()
    {
        if (currentStructure != null)
        {
            if (currentStructure.WithdrawResource(transferStepAmount))
            {
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        if (currentStructure == null) return;

        var res = currentStructure.TargetResource;

        if (resourceNameText != null)
            resourceNameText.text = res.resourceType.ToString();

        if (capacityText != null)
            capacityText.text = $"{res.currentAmount} / {res.maxCapacity}";

        // Lock button visually if structure doesn't allow withdrawals
        if (decreaseButton != null)
        {
            decreaseButton.interactable = currentStructure.CanWithdraw && res.currentAmount > 0;
        }

        if (increaseButton != null)
        {
            increaseButton.interactable = res.currentAmount < res.maxCapacity;
        }
    }

    private void OnCloseClicked()
    {
        GameManager.Instance.uiController.CloseAllModals();
    }
}