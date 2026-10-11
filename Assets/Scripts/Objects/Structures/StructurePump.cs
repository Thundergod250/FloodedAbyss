
public class StructurePump : Item
{
    public override void Activate()
    {
        GameManager.Instance.uiController.OpenApexPumpHub();
    }
}
