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

    [Header("Debug Status")]
    [SerializeField] private bool isWorking = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f; // Visible in Inspector!

    public override void Activate()
    {
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

        // Consume workers (fixed subtraction syntax)
        workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker - laborCost);
        
        StartCoroutine(HarvestRoutine());
    }

    private IEnumerator HarvestRoutine()
    {
        isWorking = true;
        progressPercentage = 0f;

        float elapsed = 0f;
        while (elapsed < productionDuration)
        {
            elapsed += Time.deltaTime;
            progressPercentage = Mathf.Clamp01(elapsed / productionDuration);
            yield return null;
        }

        progressPercentage = 1f;

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

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null && harvestUI.IsOpen)
        {
            harvestUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        // Despawn structure
        Destroy(gameObject);
    }

    // Public getters for the UI to read independently
    public bool IsWorking => isWorking;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public string ProductionDescription => $"Producing {productionAmount} {(resourceToProduce != null ? resourceToProduce.ResourceName : "Resource")}";
}