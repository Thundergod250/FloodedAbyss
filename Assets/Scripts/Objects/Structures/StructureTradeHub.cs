using UnityEngine;

public class StructureTradeHub : Item
{
    public override void Activate()
    {
        GameManager.Instance.uiController.OpenModal<UiTrade>();
    }
}
