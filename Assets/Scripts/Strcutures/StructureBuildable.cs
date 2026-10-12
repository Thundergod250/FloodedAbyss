using UnityEngine;

public class StructureBuildable : Structures
{
    [Header("Buildable Configuration")]
    [SerializeField] private StructureDataSO[] availableStructuresToBuild;
    [SerializeField] private GameObject buildableTerminalPrefab; // Reference to this building terminal's prefab so it can respawn later

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

        // 3. Instantiate the target structure and pass the terminal prefab reference for future respawning
        if (structureData.StructurePrefab != null)
        {
            Vector3 spawnPosition = transform.TransformPoint(spawnOffset);
            GameObject spawnedGO = Instantiate(structureData.StructurePrefab, spawnPosition, transform.rotation);

            StructureHarvestResource harvestStruct = spawnedGO.GetComponent<StructureHarvestResource>();
            if (harvestStruct != null)
            {
                if (buildableTerminalPrefab != null)
                {
                    harvestStruct.SetTerminalPrefab(buildableTerminalPrefab);
                }
                else
                {
                    Debug.LogWarning("[StructureBuildable] Buildable Terminal Prefab is not assigned in the Inspector! It won't be able to respawn.");
                }
            }

            Debug.Log($"[StructureBuildable] Successfully built {structureData.StructureTitle}. Replacing terminal.");
            
            GameManager.Instance.uiController.CloseAllModals();
            Destroy(gameObject);
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