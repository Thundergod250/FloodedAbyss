using UnityEngine;

public class ShopBuildable : ItemBuildable
{
    private void Reset()
    {
        buildableType = BuildableType.CampShop;
        SetupDefaultOption();
    }

    private void Awake()
    {
        if (buildableOptions.Count == 0)
        {
            SetupDefaultOption();
        }
    }

    private void SetupDefaultOption()
    {
        buildableType = BuildableType.CampShop;

        if (buildableOptions.Count == 0)
        {
            BuildableOption campOption = new BuildableOption
            {
                title = "Camp",
                description = "Allows you to upgrade Basic PickAxe and Player Stats.",
                resourceType = ResourceType.Stone,
                resourceCost = 10
            };
            buildableOptions.Add(campOption);
        }
    }

    public override void OnBuildingConstructed(BuildableOption option, GameObject spawnedInstance)
    {
        base.OnBuildingConstructed(option, spawnedInstance);
        Debug.Log("Camp & Shop NPC successfully constructed!");
    }
}