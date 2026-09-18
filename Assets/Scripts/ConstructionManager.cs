using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    [Header("Player Resources")]
    public int stone;
    public int wood;
    public int plastic;

    [Header("Construction Costs")]
    public int workshopStone = 20;
    public int workshopWood = 15;
    public int workshopPlastic = 10;

    public int managementStone = 10;
    public int managementWood = 10;
    public int managementPlastic = 5;

    public int craftingTableStone = 10;
    public int craftingTableWood = 15;
    public int craftingTablePlastic = 5;

    public int residentialStone = 20;
    public int residentialWood = 25;
    public int residentialPlastic = 10;

    public int farmStone = 15;
    public int farmWood = 20;
    public int farmPlastic = 10;

    public int autoMinerStone = 25;
    public int autoMinerWood = 30;
    public int autoMinerPlastic = 15;


    public bool HasEnoughResources(int requiredStone, int requiredWood, int requiredPlastic)
    {
        return stone >= requiredStone &&
               wood >= requiredWood &&
               plastic >= requiredPlastic;
    }


    public bool SpendResources(int requiredStone, int requiredWood, int requiredPlastic)
    {
        if (!HasEnoughResources(requiredStone, requiredWood, requiredPlastic))
        {
            Debug.Log("Not enough resources.");
            return false;
        }

        stone -= requiredStone;
        wood -= requiredWood;
        plastic -= requiredPlastic;

        return true;
    }


    public bool BuildWorkshop()
    {
        if (!SpendResources(workshopStone, workshopWood, workshopPlastic))
            return false;

        Debug.Log("Workshop LVL 1 Built!");

        return true;
    }


    public bool UnlockManagementScreen()
    {
        if (!SpendResources(managementStone, managementWood, managementPlastic))
            return false;

        Debug.Log("Management Screen Unlocked!");

        return true;
    }



    public bool BuildCraftingTable()
    {
        if (!SpendResources(craftingTableStone, craftingTableWood, craftingTablePlastic))
            return false;

        Debug.Log("Crafting Table LVL 1 Built!");

        return true;
    }


    public bool BuildResidential()
    {
        if (!SpendResources(residentialStone, residentialWood, residentialPlastic))
            return false;

        Debug.Log("Residential Building LVL 1 Built!");

        return true;
    }


    public bool BuildFarm()
    {
        if (!SpendResources(farmStone, farmWood, farmPlastic))
            return false;

        Debug.Log("Farm LVL 1 Built!");

        return true;
    }

    public bool BuildAutoMiner()
    {
        if (!SpendResources(autoMinerStone, autoMinerWood, autoMinerPlastic))
            return false;

        Debug.Log("Auto-Miner Built!");

        return true;
    }
}