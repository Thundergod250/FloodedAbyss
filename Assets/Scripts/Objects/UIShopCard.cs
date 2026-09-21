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

    /// <summary>
    /// Configures the shop card UI matching the UIStructureCard format.
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
        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        if (levelText != null)
            levelText.text = levelInfo;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        if (costText != null)
            costText.text = costInfo;

        // Toggle Max Level overlay
        if (maxLevelOverlay != null)
            maxLevelOverlay.SetActive(isMaxLevel);

        // Configure Buy Button
        if (buyButton != null)
        {
            buyButton.interactable = canAfford && !isMaxLevel;
            buyButton.onClick.RemoveAllListeners();

            if (canAfford && !isMaxLevel && onBuyClicked != null)
            {
                buyButton.onClick.AddListener(() => onBuyClicked.Invoke());
            }
        }
    }
}