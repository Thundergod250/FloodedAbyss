using UnityEngine;
using TMPro;

public class CraftingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;

    public void CloseCraftingMenu()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.HideCrafting();
        }
    }

    public void CraftPlaceholderItem(string itemName)
    {
        if (statusText != null)
        {
            statusText.text = $"Crafted: {itemName}!";
        }
        Debug.Log($"Placeholder: Crafted {itemName}");
    }
}