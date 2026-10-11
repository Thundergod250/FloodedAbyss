using UnityEngine;

public class StructureUpgradeHub : Item
{
    public override void Activate()
    {
        GameManager.Instance.uiController.OpenUpgradeHub();
    }
}