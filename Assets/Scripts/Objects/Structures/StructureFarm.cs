using System.Collections;
using UnityEngine;

public class StructureFarm : Structures
{
    [Header("Farm Info")]
    [SerializeField] private string structureTitle = "Farm";
    [SerializeField] private int laborCost = 1;
    [SerializeField] private float productionDuration = 5f;
    [SerializeField] private float destructionDuration = 3f;

    [Header("Destruction Settings")]
    [SerializeField] private bool canBeDestroyed = true;

    [Header("Food Production")]
    [SerializeField] private ResourceDataSO foodResource;
    [SerializeField] private int foodPerCycle = 1;

    [Header("Debug Status")]
    [SerializeField] private bool isWorking = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    private Coroutine workRoutine;
    private Coroutine destroyRoutine;

    // ------------------------------------------------------------------
    // UI ACCESS (mirrors existing pattern; UiFarm lives on the UI root)
    // ------------------------------------------------------------------
    private UiFarm GetFarmUI()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.uiController == null)
        {
            return null;
        }

        return GameManager.Instance.uiController.UiFarm;
    }

    public override void Activate()
    {
        if (isSubmerged && !ignoreSubmersion)
            return;

        var farmUI = GetFarmUI();

        if (farmUI == null)
        {
            Debug.LogWarning("[StructureFarm] UiFarm reference not found!");
            return;
        }

        farmUI.Setup(this);
        GameManager.Instance.uiController.OpenModal<UiFarm>();
    }

    protected override void HandleSubmergedShutdown()
    {
        base.HandleSubmergedShutdown();
        if (ignoreSubmersion) return;
        ExecuteFinalDestruction();
    }

    // ------------------------------------------------------------------
    // WORK CONTROLS
    // ------------------------------------------------------------------
    public void ToggleWorking()
    {
        if (isDestroying) return;
        if (isWorking) StopWorking(true);
        else TryStartWorking();
    }

    public void TryStartWorking()
    {
        if (isWorking || isDestroying) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var farmUI = GetFarmUI();
        if (workerMgr == null) return;

        if (workerMgr.CurrentWorker < laborCost)
        {
            if (farmUI != null) farmUI.ShowNotEnoughWorkersWarning();
            return;
        }

        workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker - laborCost);
        workRoutine = StartCoroutine(WorkLoopRoutine());
    }

    public void StopWorking(bool returnWorkers)
    {
        if (!isWorking) return;

        if (workRoutine != null)
        {
            StopCoroutine(workRoutine);
            workRoutine = null;
        }

        if (returnWorkers)
        {
            var workerMgr = GameManager.Instance.WorkerManager;
            if (workerMgr != null)
                workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker + laborCost);
        }

        isWorking = false;
        progressPercentage = 0f;
    }

    // ------------------------------------------------------------------
    // DESTRUCTION
    // ------------------------------------------------------------------
    public void ToggleDestroying()
    {
        if (!canBeDestroyed) return;
        if (isDestroying) CancelDestroying();
        else StartDestroying();
    }

    private void StartDestroying()
    {
        if (!canBeDestroyed || isDestroying) return;
        if (isWorking) StopWorking(true);

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
        ForceCleanupAndDestroy();
    }

    public void ForceCleanupAndDestroy()
    {
        if (isWorking) StopWorking(true);
        else if (workRoutine != null)
        {
            StopCoroutine(workRoutine);
            workRoutine = null;
        }

        var farmUI = GetFarmUI();
        if (farmUI != null && farmUI.IsShowingStructure(this))
        {
            farmUI.CompleteProgress();
            farmUI.Hide();
        }

        Destroy(gameObject);
    }

    // ------------------------------------------------------------------
    // PRODUCTION LOOP
    // ------------------------------------------------------------------
    private IEnumerator WorkLoopRoutine()
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

            if (foodResource != null && GameManager.Instance.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.AddResource(foodResource, foodPerCycle);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    // ------------------------------------------------------------------
    // GETTERS (used by UiFarm)
    // ------------------------------------------------------------------
    public bool IsWorking => isWorking;
    public bool IsDestroying => isDestroying;
    public bool CanBeDestroyed => canBeDestroyed;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public int FoodPerCycle => foodPerCycle;
    public ResourceDataSO FoodResource => foodResource;
    public string ProductionDescription => isDestroying
        ? "Destroying Farm..."
        : $"Producing {foodPerCycle} {(foodResource != null ? foodResource.ResourceName : "Food")}";
}