using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UiUpgradeRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image imgIcon;
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtLevel;       
    [SerializeField] private TMP_Text txtCost;        
    [SerializeField] private Slider sliderProgressBar;
    [SerializeField] private Button btnUpgrade;

    [Header("Events")]
    public UnityEvent EvtOnUpgradeClicked;

    public void SetupRow(
        Sprite icon, 
        string title, 
        int currentLevel, 
        int maxLevel, 
        int cost, 
        string costResourceName, 
        bool canAfford, 
        UnityAction onClickAction)
    {
        if (imgIcon != null && icon != null) imgIcon.sprite = icon;
        if (txtTitle != null) txtTitle.text = title;
        
        // Displays level starting at Lvl 1 default
        if (txtLevel != null) txtLevel.text = $"Lvl {currentLevel}";

        // Progress Bar
        if (sliderProgressBar != null)
        {
            sliderProgressBar.minValue = 1; // Set min to 1 to align with starting level
            sliderProgressBar.maxValue = maxLevel;
            sliderProgressBar.value = currentLevel;
        }

        // Button and Cost Text
        bool isMaxLevel = currentLevel >= maxLevel;

        if (txtCost != null)
        {
            txtCost.text = isMaxLevel ? "MAX" : $"{cost} {costResourceName}";
        }

        // Setup UnityEvent listener
        if (EvtOnUpgradeClicked == null)
            EvtOnUpgradeClicked = new UnityEvent();

        EvtOnUpgradeClicked.RemoveAllListeners();
        if (onClickAction != null)
        {
            EvtOnUpgradeClicked.AddListener(onClickAction);
        }

        if (btnUpgrade != null)
        {
            btnUpgrade.onClick.RemoveAllListeners();
            btnUpgrade.onClick.AddListener(() => EvtOnUpgradeClicked?.Invoke());
            btnUpgrade.interactable = canAfford && !isMaxLevel;
        }
    }
}