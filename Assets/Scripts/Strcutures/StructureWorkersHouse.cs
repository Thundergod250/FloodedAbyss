using System.Collections;
using UnityEngine;

public class StructureWorkersHouse : Structures
{
    [Header("House Info")]
    [SerializeField] private string structureTitle = "Worker's House";
    [SerializeField] private float actionDuration = 3f;
    [SerializeField] private float destructionDuration = 3f;

    [Header("Recruitment Costs (When no homeless workers)")]
    [SerializeField] private ResourceDataSO recruitmentCostResource;
    [SerializeField] private int recruitmentCostAmount = 5;

    
    [Header("Debug Status")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private bool isWorking = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField] private bool isReservedForHousing = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    private Coroutine actionRoutine;
    private Coroutine destroyRoutine;
    private StructureBuildable owningTerminal;

    public void SetOwningTerminal(StructureBuildable terminal)
    {
        owningTerminal = terminal;
    }

    public override void Activate()
    {
        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null)
        {
            houseUI.Setup(this);
            GameManager.Instance.uiController.OpenModal<UiWorkersHouse>();
        }
    }

    protected override void HandleSubmergedShutdown()
    {
        base.HandleSubmergedShutdown();

        if (isWorking)
        {
            StopAction();
        }

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    public void ToggleHouseAction()
    {
        if (isOccupied || isDestroying || isSubmerged) return;

        if (isWorking)
        {
            StopAction();
        }
        else
        {
            TryStartAction();
        }
    }

    private void TryStartAction()
    {
        if (isWorking || isOccupied || isSubmerged) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr == null) return;

        bool housingMode = workerMgr.HomelessWorker > 0;

        if (housingMode)
        {
            if (!workerMgr.TryHouseWorker())
            {
                Debug.Log("[StructureWorkersHouse] No homeless workers available to house!");
                return;
            }
            isReservedForHousing = true;
        }
        else
        {
            if (recruitmentCostResource != null)
            {
                var resourceMgr = GameManager.Instance.ResourceManager;
                if (resourceMgr != null)
                {
                    if (resourceMgr.GetResourceAmount(recruitmentCostResource) < recruitmentCostAmount)
                    {
                        Debug.Log("[StructureWorkersHouse] Not enough resources to recruit worker!");
                        return;
                    }
                    resourceMgr.ConsumeResource(recruitmentCostResource, recruitmentCostAmount);
                }
            }
        }

        actionRoutine = StartCoroutine(ActionRoutine(housingMode));
    }
   

    private void StopAction()
    {
        if (!isWorking) return;

        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }

        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            if (isReservedForHousing)
            {
                workerMgr.CancelHouseReservation(); 
                isReservedForHousing = false;
            }
            else if (recruitmentCostResource != null)
            {
                GameManager.Instance.ResourceManager.AddResource(recruitmentCostResource, recruitmentCostAmount);
            }
        }

        isWorking = false;
        progressPercentage = 0f;
    }

    private IEnumerator ActionRoutine(bool housingMode)
    {
        isWorking = true;
        progressPercentage = 0f;
        float elapsed = 0f;

        while (elapsed < actionDuration)
        {
            if (isDestroying || isSubmerged) yield break;
            elapsed += Time.deltaTime;
            progressPercentage = Mathf.Clamp01(elapsed / actionDuration);
            yield return null;
        }

        progressPercentage = 1f;
        ExecuteCompletion(housingMode);
    }

    private void ExecuteCompletion(bool housingMode)
    {
        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            if (!housingMode)
            {
                workerMgr.RecruitNewWorker(this);
            }
        }

        isOccupied = true;
        isWorking = false;
        isReservedForHousing = false;

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            houseUI.CompleteProgress();
        }
    }

    public void ToggleDestroying()
    {
        if (isDestroying || isSubmerged)
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
        if (isDestroying || isSubmerged) return;
        if (isWorking) StopAction();

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
            if (isSubmerged) yield break;
            elapsed += Time.deltaTime;
            progressPercentage = Mathf.Clamp01(elapsed / destructionDuration);
            yield return null;
        }

        progressPercentage = 1f;
        ExecuteFinalDestruction();
    }

    private void ExecuteFinalDestruction()
    {
        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            if (isOccupied || isReservedForHousing)
            {
                workerMgr.FreeHouse();
            }
        }

        if (owningTerminal != null)
        {
            owningTerminal.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[StructureWorkersHouse] '{gameObject.name}' has no owning terminal reference to re-enable!");
        }

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            houseUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        Destroy(gameObject);
    }

    
    // Getters
    public bool IsOccupied => isOccupied;
    public bool IsWorking => isWorking;
    public bool IsDestroying => isDestroying;
    public bool IsReservedForHousing => isReservedForHousing;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public ResourceDataSO RecruitmentCostResource => recruitmentCostResource;
    public int RecruitmentCostAmount => recruitmentCostAmount;
    public string ProgressDescription => isDestroying ? "Destroying House..." : (isReservedForHousing ? "Housing Worker..." : "Recruiting Worker...");
}