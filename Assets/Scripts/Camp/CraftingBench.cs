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
        if (UIController.Instance != null)
        {
            UIController.Instance.ShowCrafting();
        }
    }

    public void CloseCrafting()
    {
        isCraftingOpen = false;
        if (UIController.Instance != null)
        {
            UIController.Instance.HideCrafting();
        }
    }
}