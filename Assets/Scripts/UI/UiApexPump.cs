using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiApexPump : UiModals
{
    [Header("UI Containers")]
    [SerializeField] private GameObject resourceButtonContainer;
    [SerializeField] private GameObject amountButtonContainer;

    [Header("Display References")]
    [SerializeField] private TextMeshProUGUI currentFuelValueText;

    [Header("Interactive Buttons")]
    [SerializeField] private Button[] resourceButtons; // 5 buttons for Fiber, Charcoal, Biofuel, Crude Oil, Refined Fuel
    [SerializeField] private Button[] amountButtonInstances; // 10, 25, 50, 100 buttons
    [SerializeField] private Button[] switchButtons; // 5 Switch buttons

    private StructureApexPump currentPump;
    private int selectedResourceIndex = -1;

    protected override void Initialize()
    {
        base.Initialize();

        // Setup Resource Buttons
        for (int i = 0; i < resourceButtons.Length; i++)
        {
            int index = i;
            resourceButtons[i].onClick.AddListener(() => OnResourceSelected(index));
        }

        // Setup Amount Buttons
        int[] amounts = { 10, 25, 50, 100 };
        for (int i = 0; i < amountButtonInstances.Length; i++)
        {
            int amount = amounts[i];
            amountButtonInstances[i].onClick.AddListener(() => OnAmountSelected(amount));
        }

        // Setup Switch Buttons
        for (int i = 0; i < switchButtons.Length; i++)
        {
            int index = i;
            switchButtons[i].onClick.AddListener(() => OnSwitchClicked(index));
        }
    }

    private void Update()
    {
        if (IsOpen && currentPump != null)
        {
            // Update Fuel Text
            if (currentFuelValueText != null)
                currentFuelValueText.text = currentPump.CurrentFuel.ToString();

            // Refresh Switch Colors (Red if inactive, Green if active)
            for (int i = 0; i < switchButtons.Length; i++)
            {
                var colors = switchButtons[i].colors;
                colors.normalColor = currentPump.IsSwitchActive(i) ? Color.green : Color.red;
                colors.highlightedColor = colors.normalColor;
                switchButtons[i].colors = colors;
            }
        }
    }

    public void Setup(StructureApexPump pump)
    {
        currentPump = pump;
        selectedResourceIndex = -1;

        // Show Resource selection, hide Amount selection initially
        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(false);
    }

    private void OnResourceSelected(int resourceIndex)
    {
        selectedResourceIndex = resourceIndex;

        // Reveal the Amount container once resource is chosen
        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(true);
    }

    private void OnAmountSelected(int amount)
    {
        if (currentPump != null && selectedResourceIndex != -1)
        {
            currentPump.ConvertResourceToFuel(selectedResourceIndex, amount);
        }
    }

    private void OnSwitchClicked(int switchIndex)
    {
        if (currentPump != null)
        {
            currentPump.ToggleSwitch(switchIndex);
        }
    }
}