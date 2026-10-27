using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShopCard : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Image References")]
    [SerializeField] private Image iconImage;

    [Header("Cost & Purchase Setup")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;

    [Header("Max Level / Lock Overlay")]
    [SerializeField] private GameObject maxLevelOverlay;

    private PlayerStats playerStats;

    [Header("Upgrade Type")]
    [SerializeField] private UpgradeType upgradeType;


    public enum UpgradeType
    {
        Health,
        Stamina,
        Oxygen,
        AxeDamage,
        AxeSpeed
    }


    private void Start()
    {
        // Automatically find PlayerStats if it was not assigned
        if (playerStats == null)
        {
            playerStats = GameManager.Instance.playerController.PlayerStats;
        }

        if (playerStats == null)
        {
            Debug.LogError(
                "UIShopCard: PlayerStats could not be found in the scene."
            );
        }
    }


    /// <summary>
    /// Configures the shop card UI.
    /// </summary>
    public void SetupCard(
        string title,
        Sprite icon,
        string description,
        string levelInfo,
        string costInfo,
        bool canAfford,
        bool isMaxLevel,
        Action onBuyClicked)
    {
        // -------------------------
        // TEXT
        // -------------------------

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (levelText != null)
            levelText.text = levelInfo;

        if (costText != null)
            costText.text = costInfo;


        // -------------------------
        // ICON
        // -------------------------

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }


        // -------------------------
        // MAX LEVEL OVERLAY
        // -------------------------

        if (maxLevelOverlay != null)
        {
            maxLevelOverlay.SetActive(isMaxLevel);
        }


        // -------------------------
        // BUY BUTTON
        // -------------------------

        if (buyButton != null)
        {
            // Remove old listeners first
            buyButton.onClick.RemoveAllListeners();

            // Enable/disable button
            buyButton.interactable = canAfford && !isMaxLevel;

            // Only allow purchase if:
            // 1. Player can afford it
            // 2. Upgrade isn't maxed
            if (canAfford && !isMaxLevel)
            {
                buyButton.onClick.AddListener(() =>
                {
                    PurchaseUpgrade(onBuyClicked);
                });
            }
        }
    }


    /// <summary>
    /// Handles the purchase and then upgrades the player's stat.
    /// </summary>
    private void PurchaseUpgrade(Action onBuyClicked)
    {
        if (playerStats == null)
        {
            Debug.LogError(
                "UIShopCard: PlayerStats reference is missing."
            );

            return;
        }


        // --------------------------------
        // FIRST: HANDLE RESOURCE PURCHASE
        // --------------------------------

        if (onBuyClicked != null)
        {
            onBuyClicked.Invoke();
        }


        // --------------------------------
        // SECOND: UPGRADE PLAYER STAT
        // --------------------------------

        UpgradePlayerStat();


        // --------------------------------
        // DEBUG
        // --------------------------------

        Debug.Log(
            "Purchased upgrade: " + upgradeType
        );
    }


    /// <summary>
    /// Calls the correct PlayerStats upgrade method.
    /// </summary>
    private void UpgradePlayerStat()
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:

                playerStats.UpgradeMaxHealth();

                break;


            case UpgradeType.Stamina:

                playerStats.UpgradeMaxStamina();

                break;


            case UpgradeType.Oxygen:

                playerStats.UpgradeMaxOxygen();

                break;


            case UpgradeType.AxeDamage:

                playerStats.UpgradePickAxeDamage();

                break;


            case UpgradeType.AxeSpeed:

                playerStats.UpgradePickAxeAttackSpeed();

                break;


            default:

                Debug.LogWarning(
                    "UIShopCard: No upgrade type selected."
                );

                break;
        }
    }
}