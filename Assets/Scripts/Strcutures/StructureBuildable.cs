using UnityEngine;

public class StructureBuildable : Structures
{
    [Header("Buildable Configuration")]
    [SerializeField] private StructureDataSO[] availableStructuresToBuild;
    [SerializeField] private int laborCost = 1; // Default labor cost to build/operate this structure

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    public override void Activate()
    {
        // Prevent opening if already submerged
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
        base.HandleSubmergedShutdown();

        // Force close UI if it's currently showing this buildable terminal
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

        // 1. Check available workers
        if (workerMgr.CurrentWorker < laborCost)
        {
            errorMessage = $"Not enough workers! Need {laborCost}, have {workerMgr.CurrentWorker}.";
            return false;
        }

        // 2. Check resources
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

        // 3. Consume workers (fixed syntax here)
        workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker - laborCost);

        // 4. Consume resources
        foreach (var cost in structureData.Costs)
        {
            if (cost.resourceDataSo == null) continue;
            resourceManager.ConsumeResource(cost.resourceDataSo, cost.amount);
        }

        // 5. Instantiate the target structure and pass both the terminal and labor cost
        if (structureData.StructurePrefab != null)
        {
            Vector3 spawnPosition = transform.TransformPoint(spawnOffset);
            GameObject spawnedGO = Instantiate(structureData.StructurePrefab, spawnPosition, transform.rotation);

            StructureWorkersHouse house = spawnedGO.GetComponent<StructureWorkersHouse>();

            if (house != null)
            {
                house.SetCurrentPrefab(structureData.StructurePrefab); 
            }

            StructureHarvestResource harvestStruct = spawnedGO.GetComponent<StructureHarvestResource>();
            if (harvestStruct != null)
            {
                harvestStruct.SetOwningTerminal(this);
            }

            Debug.Log($"[StructureBuildable] Successfully built {structureData.StructureTitle}. Disabling terminal.");
            
            GameManager.Instance.uiController.CloseAllModals();
            
            // Disable this terminal to save performance and retain state
            gameObject.SetActive(false);
        }
        else
        {
            errorMessage = "Structure prefab is missing!";
            // Refund workers if instantiation fails
            workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker + laborCost);
            return false;
        }

        return true;
    }

    public StructureDataSO[] AvailableStructuresToBuild => availableStructuresToBuild;
    public int LaborCost => laborCost;
}