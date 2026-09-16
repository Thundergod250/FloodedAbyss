using UnityEngine;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Upgrade Cost Settings")]
    [SerializeField] private ResourceType costResourceType = ResourceType.Stone;
    [SerializeField] private int baseCost = 10;
    [SerializeField] private int costIncreasePerLevel = 5;
    [SerializeField] private int damageIncreaseAmount = 5;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject buyDamageButton;

    private PlayerResources playerResources;
    private TemporaryAttack playerAttack;
    private int currentUpgradeLevel = 0;

    private void OnEnable()
    {
        FindPlayerReferences();
        UpdateShopUI();
    }

    private void FindPlayerReferences()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.GetComponent<PlayerResources>();
            playerAttack = GameManager.Instance.playerController.GetComponent<TemporaryAttack>();
        }
        else
        {
            playerResources = FindAnyObjectByType<PlayerResources>();
            playerAttack = FindAnyObjectByType<TemporaryAttack>();
        }
    }

    public void PurchaseDamageUpgrade()
    {
        if (playerResources == null || playerAttack == null) return;

        int currentCost = GetCurrentCost();

        if (playerResources.SpendResource(costResourceType, currentCost))
        {
            currentUpgradeLevel++;
            playerAttack.AddDamage(damageIncreaseAmount);
            playerResources.UpdateResourceText();
            UpdateShopUI();
        }
        else
        {
            Debug.Log("Not enough resources for upgrade!");
        }
    }

    private int GetCurrentCost()
    {
        return baseCost + (currentUpgradeLevel * costIncreasePerLevel);
    }

    public void UpdateShopUI()
    {
        if (playerAttack != null && damageText != null)
        {
            damageText.text = $"Current Damage: {playerAttack.CurrentDamage}";
        }

        if (costText != null)
        {
            costText.text = $"Upgrade Cost: {GetCurrentCost()} {costResourceType}";
        }
    }
}