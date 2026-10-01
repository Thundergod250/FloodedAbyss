using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiApexPump : UiModals
{
    [Header("UI Controls")]
    [SerializeField] private Button btnDepositOne;  // ">" button (Deposit 1)[cite: 3]
    [SerializeField] private Button btnDepositTen;  // ">>" button (Deposit 10)[cite: 3]

    [Header("UI Displays")]
    [SerializeField] private TMP_Text txtStoneDeposited; // Displays currently deposited stone waiting to be eaten[cite: 3]
    [SerializeField] private TMP_Text txtEnergyValue;    // Displays current energy level[cite: 3]

    [Header("Pump Settings")]
    [SerializeField] private float convertInterval = 2f;    // Time in seconds to consume 1 Stone and convert to 1 Energy
    [SerializeField] private float energyDecayInterval = 5f; // Time in seconds to lose 1 Energy

    private int depositedStone = 0;
    private int currentEnergy = 0;

    private float convertTimer = 0f;
    private float decayTimer = 0f;

    // Cached reference to avoid repeated hierarchy lookups
    private PlayerResources playerResources;

    protected override void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null) 
            playerResources = GameManager.Instance.playerController.PlayerResources;
        
        if (playerResources == null)
        {
            Debug.LogError("UiApexPump: PlayerResources could not be found!");
            return;
        }

        // Bind deposit button listeners[cite: 3]
        btnDepositOne.onClick.AddListener(() => DepositStone(1));
        btnDepositTen.onClick.AddListener(() => DepositStone(10));

        UpdateUI();
    }

    private void Update()
    {
        HandleStoneToEnergyConversion();
        HandleEnergyDecay();
    }

    private void DepositStone(int amount)
    {
        if (playerResources == null) return;

        // Verify player has enough Stone available to deposit
        if (playerResources.GetResource(ResourceType.Stone) >= amount)
        {
            if (playerResources.SpendResource(ResourceType.Stone, amount))
            {
                depositedStone += amount;
                UpdateUI();
            }
        }
        else
        {
            Debug.LogWarning("Not enough Stone to deposit.");
        }
    }

    private void HandleStoneToEnergyConversion()
    {
        // Immediately start eating stone over time if any is deposited
        if (depositedStone > 0)
        {
            convertTimer += Time.deltaTime;
            if (convertTimer >= convertInterval)
            {
                convertTimer = 0f;
                depositedStone--;
                currentEnergy++;
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
        // Gradually lose energy over time
        if (currentEnergy > 0)
        {
            decayTimer += Time.deltaTime;
            if (decayTimer >= energyDecayInterval)
            {
                decayTimer = 0f;
                currentEnergy--;
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
        if (txtStoneDeposited != null) txtStoneDeposited.text = depositedStone.ToString();
        if (txtEnergyValue != null) txtEnergyValue.text = currentEnergy.ToString();
    }

    private void OnDestroy()
    {
        btnDepositOne.onClick.RemoveAllListeners();
        btnDepositTen.onClick.RemoveAllListeners();
    }
}