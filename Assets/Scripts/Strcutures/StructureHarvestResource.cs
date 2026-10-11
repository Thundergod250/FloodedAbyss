using UnityEngine;
using System.Collections;

public class StructureHarvestResource : Structures
{
    [Header("Structure Info")]
    [SerializeField] private string structureTitle = "Wood Source";
    [SerializeField] private int laborCost = 1;
    [SerializeField] private float productionDuration = 3f;

    [Header("Production Reward")]
    [SerializeField] private ResourceData resourceToProduce; // Drag your Timber asset here in Inspector
    [SerializeField] private int productionAmount = 1;

    private bool isWorking = false;

    public override void Activate()
    {
        if (isWorking) return;

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null)
        {
            harvestUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiResourceHarvest>();
        }
    }

    public void TryStartHarvest()
    {
        if (isWorking) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;

        if (workerMgr == null) return;

        if (workerMgr.CurrentWorker < laborCost)
        {
            if (harvestUI != null)
                harvestUI.ShowNotEnoughWorkersWarning();
            return;
        }

        // Consume workers
        workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker - laborCost);
        
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

        // Give the resource via ResourceManager!
        if (resourceToProduce != null && GameManager.Instance.ResourceManager != null)
        {
            GameManager.Instance.ResourceManager.AddResource(resourceToProduce, productionAmount);
        }

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

        // Despawn structure
        Destroy(gameObject);
    }

    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public string ProductionDescription => $"Producing {productionAmount} {(resourceToProduce != null ? resourceToProduce.ResourceName : "Resource")}";
}