using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStructures : UiModals
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text resourceNameText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text extraStatusText; // Displays current player energy info
    [SerializeField] private Button increaseButton;   // '>' button
    [SerializeField] private Button decreaseButton;  // '<' button

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

    public void UpdateUI()
    {
        if (currentStructure == null) return;

        var res = currentStructure.TargetResource;

        if (resourceNameText != null)
            resourceNameText.text = res.resourceType.ToString();

        if (capacityText != null)
            capacityText.text = $"{res.currentAmount} / {res.maxCapacity}";

        // Show total player Energy when inspecting a Generator
        if (extraStatusText != null)
        {
            if (currentStructure is StructureGenerator)
            {
                extraStatusText.gameObject.SetActive(true);

                // Fetch total current Energy from PlayerResources
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