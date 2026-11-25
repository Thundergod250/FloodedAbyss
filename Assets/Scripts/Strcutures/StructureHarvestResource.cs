using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public interface IWorkerUser
{
    int LaborCost { get; }
    void ForceStopByWorkerShortage();
}

public enum HarvestMode { Consumable, Depletable, Infinite }

public class StructureHarvestResource : Structures, IWorkerUser
{
    // Matches the SFXConfirm child GameObject in SoundManager.prefab.
    private const string HarvestConfirmSFXName = "SFXConfirm";

    [Header("Structure Info")]
    [SerializeField] private string structureTitle = "Wood Source";
    [SerializeField] private int laborCost = 1;
    [SerializeField] private float productionDuration = 3f;
    [SerializeField] private float destructionDuration = 3f;
    [SerializeField] private float activateCooldown = 0.5f;

    [Header("Destruction Settings")]
    [SerializeField] private bool canBeDestroyed = true;

    [Header("Harvest Behavior Mode")]
    [SerializeField] private HarvestMode harvestMode = HarvestMode.Consumable;
    [SerializeField] private int maxHarvestCycles = 5;
    private int completedHarvests = 0;

    [Header("Production Reward")]
    [SerializeField] private ResourceDataSO resourceToProduce;
    [SerializeField] private int productionAmount = 1;

    [Header("Overworld Gain Popup")]
    [SerializeField] private UIOverworld_ResourcePair overworldGainPrefab;
    [SerializeField] private Vector3 popupSpawnOffset = new Vector3(0f, 3f, 0f);

    [Header("Events")]
    [HideInInspector] public UnityEvent EvtOnBuildingWorking;
    [HideInInspector] public UnityEvent EvtOnBuildingNotWorking;

    [Header("Debug Status")]
    [SerializeField] private bool isWorking = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    private Coroutine harvestRoutine;
    private Coroutine destroyRoutine;
    private float nextAllowedActivateTime = 0f;

    protected override void Awake()
    {
        base.Awake();
        if (harvestMode == HarvestMode.Consumable) ignoreSubmersion = true;
    }

    public override void Activate()
    {
        if (isSubmerged && !ignoreSubmersion) return;

        // Consumables toggle harvest directly on click instead of opening the harvest UI
        if (harvestMode == HarvestMode.Consumable)
        {
            if (isDestroying) return;
            if (Time.unscaledTime < nextAllowedActivateTime) return;

            nextAllowedActivateTime = Time.unscaledTime + activateCooldown;

            if (isWorking)
                StopHarvesting(true);
            else
                TryStartHarvest();

            return;
        }

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null)
        {
            harvestUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiResourceHarvest>();
        }
    }

    protected override void HandleSubmergedShutdown()
    {
        if (ignoreSubmersion) return;
        ExecuteFinalDestruction();
    }

    public void ToggleHarvesting()
    {
        if (isDestroying) return;
        if (isWorking) StopHarvesting(true);
        else TryStartHarvest();
    }

    public void TryStartHarvest()
    {
        if (isWorking || isDestroying) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;

        if (workerMgr == null) return;

        // Use the new priority worker consumption system
        if (!workerMgr.TryConsumeWorkersForLabor(laborCost))
        {
            if (harvestMode != HarvestMode.Consumable && harvestUI != null)
                harvestUI.ShowNotEnoughWorkersWarning();
            return;
        }

        workerMgr.RegisterActiveWorkplace(this);

        if (ObjectiveManager.Instance != null)
            ObjectiveManager.Instance.NotifyWorkersAssigned(this, laborCost);

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

        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            workerMgr.UnregisterActiveWorkplace(this);
            if (returnWorkers)
            {
                workerMgr.ReturnWorkersFromLabor(laborCost);
            }
        }

        isWorking = false;
        progressPercentage = 0f;

        EvtOnBuildingNotWorking?.Invoke();
    }

    public void ForceStopByWorkerShortage()
    {
        if (harvestRoutine != null)
        {
            StopCoroutine(harvestRoutine);
            harvestRoutine = null;
        }

        isWorking = false;
        progressPercentage = 0f;
        // Workers perished with their housing or had to be unassigned due to deficits, so do not return them here!

        EvtOnBuildingNotWorking?.Invoke();

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null && harvestUI.IsShowingStructure(this))
        {
            harvestUI.CompleteProgress();
        }
    }

    public void ToggleDestroying()
    {
        if (!CanBeDestroyed || harvestMode == HarvestMode.Consumable) return;
        if (isDestroying) CancelDestroying();
        else StartDestroying();
    }

    private void StartDestroying()
    {
        if (!CanBeDestroyed || isDestroying) return;
        if (isWorking) StopHarvesting(true);

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

    private void ExecuteFinalDestruction() => ForceCleanupAndDestroy();

    public void ForceCleanupAndDestroy()
    {
        if (isWorking)
            StopHarvesting(true);
        else if (harvestRoutine != null)
        {
            StopCoroutine(harvestRoutine);
            harvestRoutine = null;
        }

        if (isWorking)
        {
            isWorking = false;
            EvtOnBuildingNotWorking?.Invoke();
        }

        var harvestUI = GameManager.Instance.uiController.UiResourceHarvest;
        if (harvestUI != null && harvestUI.IsShowingStructure(this))
        {
            harvestUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        Destroyed();
    }

    public override void Destroyed()
    {
        if (harvestMode == HarvestMode.Consumable)
        {
            endGameStatsManager.StructuresDestroyed(-1);
        }

        base.Destroyed();
    }

    private IEnumerator HarvestLoopRoutine()
    {
        isWorking = true;
        EvtOnBuildingWorking?.Invoke();

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

            try
            {
                if (resourceToProduce != null && GameManager.Instance.ResourceManager != null)
                {
                    GameManager.Instance.ResourceManager.AddResource(resourceToProduce, productionAmount);
                    SpawnOverworldGainPopup(resourceToProduce, productionAmount);

                    if (harvestMode == HarvestMode.Consumable)
                    {
                        var sm = GameManager.Instance.SoundManager;
                        if (sm != null)
                            sm.Play(HarvestConfirmSFXName);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StructureHarvestResource] AddResource threw: {e.Message}\n{e.StackTrace}");
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

    private void SpawnOverworldGainPopup(ResourceDataSO resourceData, int amount)
    {
        if (overworldGainPrefab == null || resourceData == null) return;

        UIOverworld_ResourcePair popup = Pool.Instantiate(overworldGainPrefab, null);
        popup.SetupAndFloat(resourceData, amount, transform.position + popupSpawnOffset);
    }

    private void FinishAndDestroyAutomatically() => ForceCleanupAndDestroy();

    public bool IsWorking => isWorking;
    public bool IsDestroying => isDestroying;
    public bool CanBeDestroyed => canBeDestroyed && harvestMode != HarvestMode.Consumable;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public int LaborCost => laborCost;
    public HarvestMode HarvestMode => harvestMode;
    public string ProductionDescription => isDestroying
        ? "Destroying Building..."
        : $"Producing {productionAmount} {(resourceToProduce != null ? resourceToProduce.ResourceName : "Resource")}";

    public int LaborCostProp => laborCost;
}