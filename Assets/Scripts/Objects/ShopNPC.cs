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
        GameManager.Instance.uiController?.CloseAllModals();
    }

    public void CloseShop()
    {
        isShopOpen = false;
        GameManager.Instance.uiController?.CloseAllModals();
    }
}