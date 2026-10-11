using System.Collections;
using UnityEngine;

public enum HarvestMode
{
    Consumable,  // 1) Destroyed after 1 harvest
    Depletable,  // 2) Repeats for a set total count, then destroyed
    Infinite     // 3) Never destroyed, runs forever until turned off
}

public class StructureHarvestResource : Structures
{
    [Header("Structure Info")]
    [SerializeField] private string structureTitle = "Wood Source";
    [SerializeField] private int laborCost = 1;
    [SerializeField] private float productionDuration = 3f;

    [Header("Harvest Behavior Mode")]
    [SerializeField] private HarvestMode harvestMode = HarvestMode.Consumable;
    [SerializeField] private int maxHarvestCycles = 5; // Used only for Depletable mode
    private int completedHarvests = 0;

    [Header("Production Reward")]
    [SerializeField] private ResourceData resourceToProduce; 
    [SerializeField] private int productionAmount = 1;

    [Header("Debug Status")]
    [SerializeField] private bool isWorking = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f; 

    private Coroutine harvestRoutine;

    public override void Activate()
    {
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null)
        {
            harvestUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiResourceHarvest>();
        }
    }

    /// <summary>
    /// Called by the UI button. Acts as a toggle: starts work if idle, or stops/cancels work if active.
    /// </summary>
    public void ToggleHarvesting()
    {
        if (isWorking)
        {
            StopHarvesting(true); // Player manually stopped it, return workers
        }
        else
        {
            TryStartHarvest();
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
        
        harvestRoutine = StartCoroutine(HarvestLoopRoutine());
    }

    public void StopHarvesting(bool returnWorkers)
    {
        if (!isWorking) return;

        if (harvestRoutine != null)
        {
            StopCoroutine(harvestRoutine);
            harvestRoutine = null;
        }

        if (returnWorkers)
        {
            var workerMgr = GameManager.Instance.WorkerManager;
            if (workerMgr != null)
            {
                workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker + laborCost);
            }
        }

        isWorking = false;
        progressPercentage = 0f;
        Debug.Log($"[StructureHarvestResource] Harvesting stopped for {structureTitle}.");
    }

    private IEnumerator HarvestLoopRoutine()
    {
        isWorking = true;

        while (true)
        {
            progressPercentage = 0f;
            float elapsed = 0f;

            // Progress bar fill loop
            while (elapsed < productionDuration)
            {
                elapsed += Time.deltaTime;
                progressPercentage = Mathf.Clamp01(elapsed / productionDuration);
                yield return null;
            }

            progressPercentage = 1f;

            // Give the resource
            if (resourceToProduce != null && GameManager.Instance.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.AddResource(resourceToProduce, productionAmount);
            }

            // Handle behavior modes after 1 successful cycle
            if (harvestMode == HarvestMode.Consumable)
            {
                FinishAndDestroy();
                yield break;
            }
            else if (harvestMode == HarvestMode.Depletable)
            {
                completedHarvests++;
                if (completedHarvests >= maxHarvestCycles)
                {
                    FinishAndDestroy();
                    yield break;
                }
            }
            else if (harvestMode == HarvestMode.Infinite)
            {
                // Loops indefinitely; resets progress for the next cycle
            }

            // Small breather before resetting bar for next loop if repeating
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void FinishAndDestroy()
    {
        // Return workers permanently upon final destruction
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

        Destroy(gameObject);
    }

    // Getters for UI Integration
    public bool IsWorking => isWorking;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public HarvestMode HarvestMode => harvestMode;
    public string ProductionDescription => $"Producing {productionAmount} {(resourceToProduce != null ? resourceToProduce.ResourceName : "Resource")}";
}