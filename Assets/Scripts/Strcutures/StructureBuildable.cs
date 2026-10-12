using UnityEngine;

public class StructureBuildable : Structures
{
    [Header("Buildable Configuration")]
    [SerializeField] private StructureDataSO[] availableStructuresToBuild;

    public override void Activate()
    {
        var buildingUI = GameManager.Instance.uiController.UiBuilding;
        if (buildingUI != null)
        {
            buildingUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiBuilding>();
        }
    }

    public bool TryBuildStructure(StructureDataSO structureData, out string errorMessage)
    {
        errorMessage = string.Empty;
        if (structureData == null)
        {
            errorMessage = "Invalid structure data.";
            return false;
        }

        var resourceManager = GameManager.Instance.ResourceManager;
        if (resourceManager == null)
        {
            errorMessage = "Resource manager not found.";
            return false;
        }

        // 1. Check if player has enough resources for all costs
        foreach (var cost in structureData.Costs)
        {
            if (cost.resourceDataSo == null) continue;
            int currentAmount = resourceManager.GetResourceAmount(cost.resourceDataSo);
            if (currentAmount < cost.amount)
            {
                errorMessage = $"Not enough {cost.resourceDataSo.ResourceName}! Need {cost.amount}, have {currentAmount}.";
                return false;
            }
        }

        // 2. Consume resources
        foreach (var cost in structureData.Costs)
        {
            if (cost.resourceDataSo == null) continue;
            bool success = resourceManager.ConsumeResource(cost.resourceDataSo, cost.amount);
            if (!success)
            {
                errorMessage = $"Failed to consume {cost.resourceDataSo.ResourceName}.";
                return false;
            }
        }

        // 3. Instantiate the structure in the world
        if (structureData.StructurePrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 3f + Vector3.up * 0.5f;
            Instantiate(structureData.StructurePrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"[StructureBuildable] Successfully built {structureData.StructureTitle}!");
        }
        else
        {
            errorMessage = "Structure prefab is missing!";
            return false;
        }

        return true;
    }

    public StructureDataSO[] AvailableStructuresToBuild => availableStructuresToBuild;
}