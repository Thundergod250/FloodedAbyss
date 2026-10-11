using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UiApexPump : UiModals
{
    [Header("UI Containers")]
    [SerializeField] private GameObject resourceButtonContainer;
    [SerializeField] private GameObject amountButtonContainer;

    [Header("Display References")]
    [SerializeField] private TextMeshProUGUI currentFuelValueText;
    [SerializeField] private TextMeshProUGUI notEnoughResourceText;

    [Header("Interactive Buttons")]
    [SerializeField] private Button[] resourceButtons; // 5 buttons for Fiber, Charcoal, Biofuel, Crude Oil, Refined Fuel
    [SerializeField] private Button[] amountButtonInstances; // 10, 25, 50, 100 buttons
    [SerializeField] private Button[] switchButtons; // 5 Switch buttons

    private StructureApexPump currentPump;
    private int selectedResourceIndex = -1;
    private Coroutine warningRoutine;

    protected override void Initialize()
    {
        base.Initialize();

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);

        // Setup Resource Buttons
        for (int i = 0; i < resourceButtons.Length; i++)
        {
            int index = i;
            if (resourceButtons[i] != null)
                resourceButtons[i].onClick.AddListener(() => OnResourceSelected(index));
        }

        // Setup Amount Buttons
        int[] amounts = { 10, 25, 50, 100 };
        for (int i = 0; i < amountButtonInstances.Length; i++)
        {
            int amount = amounts[i];
            if (amountButtonInstances[i] != null)
                amountButtonInstances[i].onClick.AddListener(() => OnAmountSelected(amount));
        }

        // Setup Switch Buttons
        for (int i = 0; i < switchButtons.Length; i++)
        {
            int index = i;
            if (switchButtons[i] != null)
                switchButtons[i].onClick.AddListener(() => OnSwitchClicked(index));
        }
    }

    private void Update()
    {
        if (IsOpen && currentPump != null)
        {
            if (currentFuelValueText != null)
                currentFuelValueText.text = currentPump.CurrentFuel.ToString();

            // Refresh Switch Colors
            for (int i = 0; i < switchButtons.Length; i++)
            {
                if (switchButtons[i] != null)
                {
                    ColorBlock colors = switchButtons[i].colors;
                    Color targetColor = currentPump.IsSwitchActive(i) ? Color.green : Color.red;
                    
                    colors.normalColor = targetColor;
                    colors.highlightedColor = targetColor;
                    colors.pressedColor = targetColor;
                    colors.selectedColor = targetColor;
                    colors.disabledColor = targetColor;
                    
                    switchButtons[i].colors = colors;
                }
            }
        }
    }

    public void Setup(StructureApexPump pump)
    {
        currentPump = pump;
        selectedResourceIndex = -1;

        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(false);

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);

        RefreshResourceButtonVisuals();
    }

    private void OnResourceSelected(int resourceIndex)
    {
        selectedResourceIndex = resourceIndex;

        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(true);

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);

        RefreshResourceButtonVisuals();
    }

    private void RefreshResourceButtonVisuals()
    {
        for (int i = 0; i < resourceButtons.Length; i++)
        {
            if (resourceButtons[i] == null) continue;

            ColorBlock colors = resourceButtons[i].colors;

            if (selectedResourceIndex == -1)
            {
                // Default state before any resource is chosen
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
                colors.selectedColor = Color.white;
            }
            else if (i == selectedResourceIndex)
            {
                // Selected resource turns solid Yellow
                colors.normalColor = Color.yellow;
                colors.highlightedColor = Color.yellow;
                colors.pressedColor = Color.yellow;
                colors.selectedColor = Color.yellow;
            }
            else
            {
                // Unselected resources fade out slightly (reduced alpha), but remain fully clickable
                Color fadedColor = new Color(1f, 1f, 1f, 0.4f);
                colors.normalColor = fadedColor;
                colors.highlightedColor = new Color(1f, 1f, 1f, 0.6f);
                colors.pressedColor = new Color(1f, 1f, 1f, 0.3f);
                colors.selectedColor = fadedColor;
            }

            resourceButtons[i].colors = colors;
        }
    }

    private void OnAmountSelected(int amount)
    {
        if (currentPump != null && selectedResourceIndex != -1)
        {
            bool success = currentPump.ConvertResourceToFuel(selectedResourceIndex, amount);
            if (!success)
            {
                ShowNotEnoughResourceWarning();
            }
        }
    }

    private void ShowNotEnoughResourceWarning()
    {
        if (notEnoughResourceText != null)
        {
            if (warningRoutine != null) StopCoroutine(warningRoutine);
            warningRoutine = StartCoroutine(FlashWarningRoutine());
        }
    }

    private IEnumerator FlashWarningRoutine()
    {
        notEnoughResourceText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        notEnoughResourceText.gameObject.SetActive(false);
    }

    private void OnSwitchClicked(int switchIndex)
    {
        if (currentPump != null)
        {
            currentPump.ToggleSwitch(switchIndex);
        }
    }
}