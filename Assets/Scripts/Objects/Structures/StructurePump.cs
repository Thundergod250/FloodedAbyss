
public class StructurePump : Item
{
    public override void Activate()
    {
        // 1. Open Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.ApexPump);

        // 2. Reference UiBuilding modal instance
        UiApexPump apexPumpModal = GameManager.Instance.uiController.GetModal<UiApexPump>(UIController.UIState.ApexPump);

        // 3. Check if WaterForecast is available, if not run it
        WaterForecast waterForecast = GameManager.Instance.uiController.GetComponentInChildren<WaterForecast>();

        if (waterForecast != null)
        {
            waterForecast.EnableTimeline();
        }
    }
}
