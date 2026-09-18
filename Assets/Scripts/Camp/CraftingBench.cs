using UnityEngine;

public class CraftingBench : Item
{
    private bool isCraftingOpen = false;

    public override void Activate()
    {
        if (!isCraftingOpen)
        {
            OpenCrafting();
        }
        else
        {
            CloseCrafting();
        }
    }

    private void OpenCrafting()
    {
        isCraftingOpen = true;
        GameManager.Instance.uiController?.ShowCrafting();
    }

    public void CloseCrafting()
    {
        isCraftingOpen = false;
        GameManager.Instance.uiController?.HideCrafting();
    }
}