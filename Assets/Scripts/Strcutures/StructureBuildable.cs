using UnityEngine;

public class StructureBuildable : Structures
{
    [Header("Buildable Configuration")]
    [SerializeField] private StructureDataSO[] availableStructuresToBuild;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

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

        // 1. Check resources
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
            resourceManager.ConsumeResource(cost.resourceDataSo, cost.amount);
        }

        // 3. Instantiate the target structure and link back to this Buildable terminal
        if (structureData.StructurePrefab != null)
        {
            Vector3 spawnPosition = transform.TransformPoint(spawnOffset);
            GameObject spawnedGO = Instantiate(structureData.StructurePrefab, spawnPosition, transform.rotation);

            StructureHarvestResource harvestStruct = spawnedGO.GetComponent<StructureHarvestResource>();
            if (harvestStruct != null)
            {
                // Pass this specific buildable terminal instance so it can re-enable us later
                harvestStruct.SetOwningTerminal(this);
            }

            Debug.Log($"[StructureBuildable] Successfully built {structureData.StructureTitle}. Disabling terminal.");
            
            GameManager.Instance.uiController.CloseAllModals();
            
            // Instead of destroying, simply disable this terminal to save performance and retain state
            gameObject.SetActive(false);
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