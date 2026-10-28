using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelResourcePair : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(ResourceDataSO resourceDataSo, int initialAmount)
    {
        if (iconImage != null && resourceDataSo.Icon != null) 
            iconImage.sprite = resourceDataSo.Icon;
        UpdateAmount(initialAmount);
    }

    public void UpdateAmount(int amount)
    {
        if (amountText != null) 
            amountText.text = amount.ToString();
    }
}
