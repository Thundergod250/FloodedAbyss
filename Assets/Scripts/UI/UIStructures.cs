using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStructures : UiModals
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text resourceNameText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private Button depositButton;   // '>' button
    [SerializeField] private Button withdrawButton;  // '<' button

    [Header("Settings")]
    [SerializeField] private int transferStepAmount = 1; // Amount transferred per click

    private ItemStructure currentStructure;

    protected override void Initialize()
    {
        base.Initialize();

        if (depositButton != null)
            depositButton.onClick.AddListener(OnDepositClicked);

        if (withdrawButton != null)
            withdrawButton.onClick.AddListener(OnWithdrawClicked);
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
    }

    private void OnCloseClicked()
    {
        GameManager.Instance.uiController.CloseAllModals();
    }
}