using System.Collections;
using UnityEngine;

public enum HarvestMode
{
    Consumable,  
    Depletable,  
    Infinite     
}

public class StructureHarvestResource : Structures
{
    [Header("Structure Info")]
    [SerializeField] private string structureTitle = "Wood Source";
    [SerializeField] private int laborCost = 1;
    [SerializeField] private float productionDuration = 3f;
    [SerializeField] private float destructionDuration = 3f;

    [Header("Destruction Settings")]
    [SerializeField] private bool canBeDestroyed = true; // Set to false for natural overworld resource piles that cannot be destroyed

    [Header("Harvest Behavior Mode")]
    [SerializeField] private HarvestMode harvestMode = HarvestMode.Consumable;
    [SerializeField] private int maxHarvestCycles = 5; 
    private int completedHarvests = 0;

    [Header("Production Reward")]
    [SerializeField] private ResourceDataSO resourceToProduce; 
    [SerializeField] private int productionAmount = 1;

    [Header("Debug Status")]
    [SerializeField] private bool isWorking = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f; 

    private Coroutine harvestRoutine;
    private Coroutine destroyRoutine;
    private GameObject terminalPrefab;

    public void SetTerminalPrefab(GameObject prefab)
    {
        terminalPrefab = prefab;
    }

    public override void Activate()
    {
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null)
        {
            harvestUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiResourceHarvest>();
        }
    }

    public void ToggleHarvesting()
    {
        if (isDestroying) return;

        if (isWorking)
        {
            StopHarvesting(true); 
        }
        else
        {
            TryStartHarvest();
        }
    }

    public void TryStartHarvest()
    {
        if (isWorking || isDestroying) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;

        if (workerMgr == null) return;

        if (workerMgr.CurrentWorker < laborCost)
        {
            if (harvestUI != null)
                harvestUI.ShowNotEnoughWorkersWarning();
            return;
        }

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
    }

    public void ToggleDestroying()
    {
        if (!canBeDestroyed) return; // Prevent destroying if disallowed

        if (isDestroying)
        {
            CancelDestroying();
        }
        else
        {
            StartDestroying();
        }
    }

    private void StartDestroying()
    {
        if (!canBeDestroyed || isDestroying) return;

        if (isWorking)
        {
            StopHarvesting(true);
        }

        isDestroying = true;
        progressPercentage = 0f;
        destroyRoutine = StartCoroutine(DestroyRoutine());
    }

    public void CancelDestroying()
    {
        if (!isDestroying) return;

        if (destroyRoutine != null)
        {
            StopCoroutine(destroyRoutine);
            destroyRoutine = null;
        }

        isDestroying = false;
        progressPercentage = 0f;
    }

    private IEnumerator DestroyRoutine()
    {
        float elapsed = 0f;
        while (elapsed < destructionDuration)
        {
            elapsed += Time.deltaTime;
            progressPercentage = Mathf.Clamp01(elapsed / destructionDuration);
            yield return null;
        }

        progressPercentage = 1f;
        ExecuteFinalDestruction();
    }

    private void ExecuteFinalDestruction()
    {
        if (terminalPrefab != null)
        {
            Instantiate(terminalPrefab, transform.position, transform.rotation);
        }

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null && harvestUI.IsOpen)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }

        Destroy(gameObject);
    }

    private IEnumerator HarvestLoopRoutine()
    {
        isWorking = true;

        while (true)
        {
            progressPercentage = 0f;
            float elapsed = 0f;

            while (elapsed < productionDuration)
            {
                if (isDestroying) yield break;
                elapsed += Time.deltaTime;
                progressPercentage = Mathf.Clamp01(elapsed / productionDuration);
                yield return null;
            }

            progressPercentage = 1f;

            if (resourceToProduce != null && GameManager.Instance.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.AddResource(resourceToProduce, productionAmount);
            }

            if (harvestMode == HarvestMode.Consumable)
            {
                FinishAndDestroyAutomatically();
                yield break;
            }
            else if (harvestMode == HarvestMode.Depletable)
            {
                completedHarvests++;
                if (completedHarvests >= maxHarvestCycles)
                {
                    FinishAndDestroyAutomatically();
                    yield break;
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void FinishAndDestroyAutomatically()
    {
        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker + laborCost);
        }

        if (terminalPrefab != null)
        {
            Instantiate(terminalPrefab, transform.position, transform.rotation);
        }

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null && harvestUI.IsOpen)
        {
            harvestUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        Destroy(gameObject);
    }

    // Getters
    public bool IsWorking => isWorking;
    public bool IsDestroying => isDestroying;
    public bool CanBeDestroyed => canBeDestroyed;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public HarvestMode HarvestMode => harvestMode;
    public string ProductionDescription => isDestroying ? "Destroying Building..." : $"Producing {productionAmount} {(resourceToProduce != null ? resourceToProduce.ResourceName : "Resource")}";
}