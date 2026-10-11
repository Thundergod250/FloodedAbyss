using UnityEngine;

public class StructureFarm : Item
{
    public override void Activate()
    {
        GameManager.Instance.uiController.OpenModal<UIAutoFarm>();
    }
}
