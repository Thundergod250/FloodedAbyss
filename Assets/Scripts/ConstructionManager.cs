using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    [Header("Player Resources")]
    public int rock;
    public int wood;
    public int plastic;

    [Header("Advanced Materials")]
    public int tin;
    public int copper;
    public int iron;
    public int bronze;

    [Header("Workshop Cost")]
    public int workshopRock = 20;
    public int workshopWood = 15;
    public int workshopIron = 10;

    [Header("Auto Miner Cost")]
    public int autoMinerBronze = 1;

    [Header("Building Status")]
    public bool workshopBuilt = false;
    public bool autoMinerBuilt = false;


    public bool HasEnoughWorkshopResources()
    {
        return rock >= workshopRock &&
               wood >= workshopWood &&
               iron >= workshopIron;
    }


    public bool HasEnoughAutoMinerResources()
    {
        return bronze >= autoMinerBronze;
    }

    public bool BuildWorkshop()
    {
        if (workshopBuilt)
        {
            Debug.Log("Workshop has already been built.");
            return false;
        }

        if (!HasEnoughWorkshopResources())
        {
            Debug.Log("Not enough resources to build Workshop.");
            return false;
        }

        rock -= workshopRock;
        wood -= workshopWood;
        iron -= workshopIron;

        workshopBuilt = true;

        Debug.Log("Workshop LVL 1 Built!");

        ShowBuildingUnlocked("Workshop");

        return true;
    }

    public bool BuildAutoMiner()
    {
        if (autoMinerBuilt)
        {
            Debug.Log("Auto-Miner has already been built.");
            return false;
        }

        if (!HasEnoughAutoMinerResources())
        {
            Debug.Log("Not enough Bronze to build Auto-Miner.");
            return false;
        }

        bronze -= autoMinerBronze;

        autoMinerBuilt = true;

        Debug.Log("Auto-Miner Built!");

        ShowBuildingUnlocked("Auto-Miner");

        return true;
    }

    public bool CraftBronze()
    {

        if (tin < 1 || copper < 1)
        {
            Debug.Log("Not enough Tin or Copper to make Bronze.");
            return false;
        }

        tin -= 1;
        copper -= 1;

        bronze += 1;

        Debug.Log("1 Bronze crafted!");

        return true;
    }

    private void ShowBuildingUnlocked(string buildingName)
    {
        Debug.Log("NEW BUILDING UNLOCKED: " + buildingName);

    }
}