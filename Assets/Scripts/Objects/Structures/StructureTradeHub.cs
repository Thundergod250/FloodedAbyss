using UnityEngine;

public class StructureTradeHub : Item
{
    public override void Activate()
    {
        // 1. Open Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Trade);
    }
}
