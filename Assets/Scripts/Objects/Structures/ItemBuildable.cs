using System.Collections.Generic;
using UnityEngine;

public class ItemBuildable : Item
{
    public override void Activate()
    {
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
        /*// 1. Open the Building Modal state
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);

        // 2. Get the specific UI_BuildablePanel script reference
        UiBuilding buildableUI = GameManager.Instance.uiController.GetModal<UiBuilding>(UIController.UIState.Building);

        if (buildableUI != null)
        {
            
        }*/
    }
}