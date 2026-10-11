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
    [SerializeField] private TextMeshProUGUI notEnoughResourceText; // Assigned to your "Not Enough Resource Txt"

    [Header("Interactive Buttons")]
    [SerializeField] private Button[] resourceButtons; 
    [SerializeField] private Button[] amountButtonInstances; 
    [SerializeField] private Button[] switchButtons; 

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
    }

    private void OnResourceSelected(int resourceIndex)
    {
        selectedResourceIndex = resourceIndex;

        if (amountButtonContainer != null)
            amountButtonContainer.SetActive(true);

        if (notEnoughResourceText != null)
            notEnoughResourceText.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(3f); // Lasts for 3 seconds
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