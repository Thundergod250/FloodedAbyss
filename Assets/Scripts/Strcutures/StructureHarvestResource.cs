using UnityEngine;
using System.Collections;

public class StructureHarvestResource : Structures
{
    [Header("Structure Info")]
    [SerializeField] private string structureTitle = "Wood Source";
    [SerializeField] private int laborCost = 5;
    [SerializeField] private string productionDescription = "Producing 1 Wood";
    [SerializeField] private float productionDuration = 3f; // Better name for Working Time

    private bool isWorking = false;

    public override void Activate()
    {
        if (isWorking) return;

        // Open UI and pass structure data
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null)
        {
            harvestUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiResourceHarvest>();
            Debug.LogWarning($"[StructureHarvestResource] Harvesting {structureTitle}");
        }
        else
        {
            Debug.LogWarning("MISSING HARVEST RESOURCE");
        }
    }

    public void TryStartHarvest()
    {
        if (isWorking) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;

        if (workerMgr == null) return;

        // Check if player has enough workers
        if (workerMgr.CurrentWorker < laborCost)
        {
            if (harvestUI != null)
                harvestUI.ShowNotEnoughWorkersWarning();
            return;
        }

        // Consume workers
        workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker - laborCost);
        
        // Begin production loop
        StartCoroutine(HarvestRoutine());
    }

    private IEnumerator HarvestRoutine()
    {
        isWorking = true;
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;

        if (harvestUI != null)
            harvestUI.BeginProgress(productionDuration);

        float elapsed = 0f;
        while (elapsed < productionDuration)
        {
            elapsed += Time.deltaTime;
            if (harvestUI != null)
                harvestUI.UpdateProgress(elapsed / productionDuration);
            yield return null;
        }

        // Give Product (Placeholder logic: trigger reward or inventory add here)
        Debug.Log($"[StructureHarvestResource] Produced: {productionDescription}");

        // Return workers
        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker + laborCost);
        }

        if (harvestUI != null)
        {
            harvestUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        // Despawn structure (Starter requirement)
        Destroy(gameObject);
    }

    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public string ProductionDescription => productionDescription;
}