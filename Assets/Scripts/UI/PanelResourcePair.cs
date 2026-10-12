using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelResourcePair : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(ResourceData resourceData, int initialAmount)
    {
        if (iconImage != null && resourceData.Icon != null) 
            iconImage.sprite = resourceData.Icon;
        UpdateAmount(initialAmount);
    }

    public void UpdateAmount(int amount)
    {
        if (amountText != null) 
            amountText.text = amount.ToString();
    }
}
