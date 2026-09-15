using UnityEngine;

public class ShopNPC : Item
{
    private bool isShopOpen = false;

    public override void Activate()
    {
        if (!isShopOpen)
        {
            OpenShop();
        }
        else
        {
            CloseShop();
        }
    }

    private void OpenShop()
    {
        isShopOpen = true;
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowShop();
        }
    }

    public void CloseShop()
    {
        isShopOpen = false;
        if (UIController.Instance != null)
        {
            UIController.Instance.HideShop();
        }
    }
}