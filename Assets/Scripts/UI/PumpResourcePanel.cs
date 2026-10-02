using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PumpResourcePanel : MonoBehaviour
{
    [Header("Resource Config")]
    [SerializeField] private ResourceType resourceType;

    [Header("UI Controls")]
    [SerializeField] private Button btnDepositOne;
    [SerializeField] private Button btnDepositTen;

    [Header("UI Displays")]
    [SerializeField] private TMP_Text txtResourceDeposited;
    [SerializeField] private TMP_Text txtEnergyValue;

    [Header("Pump Settings")]
    [SerializeField] private float convertInterval = 2f;
    [SerializeField] private float energyDecayInterval = 5f;

    private int depositedResource = 0;
    private int currentEnergy = 0;

    private float convertTimer = 0f;
    private float decayTimer = 0f;

    private PlayerResources playerResources;

    public event Action<PumpResourcePanel, bool> OnActiveStateChanged;

    public ResourceType ResourceType => resourceType;
    public int CurrentEnergy => currentEnergy;
    public bool IsActive => currentEnergy > 0;

    public void Initialize(PlayerResources resources)
    {
        playerResources = resources;

        if (btnDepositOne != null) btnDepositOne.onClick.AddListener(() => DepositResource(1));
        if (btnDepositTen != null) btnDepositTen.onClick.AddListener(() => DepositResource(10));

        UpdateUI();
    }

    private void Update()
    {
        HandleConversion();
        HandleEnergyDecay();
    }

    private void DepositResource(int amount)
    {
        if (playerResources == null) return;

        if (playerResources.GetResource(resourceType) >= amount)
        {
            if (playerResources.SpendResource(resourceType, amount))
            {
                depositedResource += amount;
                UpdateUI();
            }
        }
        else
        {
            Debug.LogWarning($"Not enough {resourceType} to deposit.");
        }
    }

    private void HandleConversion()
    {
        if (depositedResource > 0)
        {
            convertTimer += Time.deltaTime;
            if (convertTimer >= convertInterval)
            {
                convertTimer = 0f;
                depositedResource--;

                bool wasActiveBefore = IsActive;

                // Increment energy before evaluating active state change
                currentEnergy++;

                if (!wasActiveBefore && IsActive)
                {
                    Debug.Log($"[{resourceType} Pump] is now ACTIVE!");
                    OnActiveStateChanged?.Invoke(this, true);
                }

                UpdateUI();
            }
        }
        else
        {
            convertTimer = 0f;
        }
    }

    private void HandleEnergyDecay()
    {
        if (currentEnergy > 0)
        {
            decayTimer += Time.deltaTime;
            if (decayTimer >= energyDecayInterval)
            {
                decayTimer = 0f;
                bool wasActiveBefore = IsActive;

                // Decrement energy before evaluating state change
                currentEnergy--;

                if (wasActiveBefore && !IsActive)
                {
                    Debug.Log($"[{resourceType} Pump] is now INACTIVE!");
                    OnActiveStateChanged?.Invoke(this, false);
                }

                UpdateUI();
            }
        }
        else
        {
            decayTimer = 0f;
        }
    }

    private void UpdateUI()
    {
        if (txtResourceDeposited != null) txtResourceDeposited.text = depositedResource.ToString();
        if (txtEnergyValue != null) txtEnergyValue.text = currentEnergy.ToString();
    }

    private void OnDestroy()
    {
        if (btnDepositOne != null) btnDepositOne.onClick.RemoveAllListeners();
        if (btnDepositTen != null) btnDepositTen.onClick.RemoveAllListeners();
    }
}