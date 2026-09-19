using UnityEngine;

public class ShopNPC : Item
{
    private bool isShopOpen = false;

    public override void Activate()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            if (!isShopOpen)
            {
                isShopOpen = true;
                GameManager.Instance.uiController.OpenModal(UIController.UIState.Shop);
            }
            else
            {
                isShopOpen = false;
                GameManager.Instance.uiController.CloseAllModals();
            }
        }
    }
}