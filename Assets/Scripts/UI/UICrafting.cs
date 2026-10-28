using UnityEngine;
using TMPro;

public class UICrafting : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;

    //public void CloseCraftingMenu() => GameManager.Instance.uiController?.HideCrafting();

    public void CraftPlaceholderItem(string itemName)
    {
        if (statusText != null)
        {
            statusText.text = $"Crafted: {itemName}!";
        }
        Debug.Log($"Placeholder: Crafted {itemName}");
    }
}