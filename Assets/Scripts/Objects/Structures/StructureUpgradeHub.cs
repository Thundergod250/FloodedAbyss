using UnityEngine;

public class StructureUpgradeHub : Item
{
    public override void Activate()
    {
        // 1. Open Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Upgrade);
    }
}
