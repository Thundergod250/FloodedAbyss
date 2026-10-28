using UnityEngine;

public class StructureBuildable : Structures
{
    [Header("Buildable Configuration")]
    [SerializeField] private StructureDataSO[] availableStructuresToBuild;
    [SerializeField] private int laborCost = 1;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    public override void Activate()
    {
        if (isSubmerged) return;

        var buildingUI = GameManager.Instance.uiController.UiBuilding;
        if (buildingUI != null)
        {
            buildingUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiBuilding>();

            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.NotifyUIOpened("BuildingUI");
            }
        }
    }

    protected override void HandleSubmergedShutdown()
    {
        // DO NOT call base.HandleSubmergedShutdown() here!
        // The Buildable terminal is meant to stay in the world. 
        // We only want to close the UI if it happens to be open.

        var buildingUI = GameManager.Instance.uiController.UiBuilding;
        if (buildingUI != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    public bool TryBuildStructure(StructureDataSO structureData, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (isSubmerged)
        {
            errorMessage = "Cannot build; terminal is submerged!";
            return false;
        }

        if (structureData == null)
        {
            errorMessage = "Invalid structure data.";
            return false;
        }

        if (structureData.StructurePrefab == null)
        {
            errorMessage = "Structure prefab is missing!";
            return false;
        }

        var resourceManager = GameManager.Instance.ResourceManager;
        if (resourceManager == null)
        {
            errorMessage = "Resource manager not found.";
            return false;
        }

        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr == null)
        {
            errorMessage = "Worker manager not found.";
            return false;
        }

        if (workerMgr.CurrentWorker < laborCost)
        {
            errorMessage = $"Not enough available workers! Need at least {laborCost}, have {workerMgr.CurrentWorker}.";
            return false;
        }

        foreach (var cost in structureData.Costs)
        {
            if (cost.resourceDataSo == null) continue;
            if (cost.amount < 0)
            {
                errorMessage = "Invalid structure resource cost.";
                return false;
            }

            int currentAmount = resourceManager.GetResourceAmount(cost.resourceDataSo);
            if (currentAmount < cost.amount)
            {
                errorMessage = $"Not enough {cost.resourceDataSo.ResourceName}! Need {cost.amount}, have {currentAmount}.";
                return false;
            }
        }

        foreach (var cost in structureData.Costs)
        {
            if (cost.resourceDataSo == null || cost.amount == 0) continue;
            if (!resourceManager.ConsumeResource(cost.resourceDataSo, cost.amount))
            {
                errorMessage = $"Failed to consume {cost.resourceDataSo.ResourceName}.";
                return false;
            }
        }

        Vector3 spawnPosition = transform.TransformPoint(spawnOffset);
        GameObject spawnedGO = Instantiate(structureData.StructurePrefab, spawnPosition, transform.rotation);

        StructureUpgrade structureUpgrade = spawnedGO.GetComponent<StructureUpgrade>();
        if (structureUpgrade != null)
        {
            structureUpgrade.SetOwningTerminal(this);
        }

        StructureHarvestResource harvestStruct = spawnedGO.GetComponent<StructureHarvestResource>();
        if (harvestStruct != null)
        {
            harvestStruct.SetOwningTerminal(this);
        }

        StructureWorkersHouse houseStruct = spawnedGO.GetComponent<StructureWorkersHouse>();
        if (houseStruct != null)
        {
            houseStruct.SetOwningTerminal(this);
        }

        Debug.Log($"[StructureBuildable] Successfully built {structureData.StructureTitle}. Disabling terminal.");

        GameManager.Instance.uiController.CloseAllModals();
        gameObject.SetActive(false); // Hide the terminal, but don't destroy it

        return true;
    }

    public StructureDataSO[] AvailableStructuresToBuild => availableStructuresToBuild;
    public int LaborCost => laborCost;
}