using UnityEngine;

public class ShopNPC : Item
{
    public override void Activate()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Shop);
        }
    }
}