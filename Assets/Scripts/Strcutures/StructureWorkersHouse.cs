using System.Collections;
using System.Collections.Generic;
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

    [Header("Upgrade Info")]
    [SerializeField] private List<StructureDataSO> houseLevels = new List<StructureDataSO>();
    [SerializeField, Range(1f, 3f)] private int houseLevelVal;

    [Header("Debug Status")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private bool isWorking = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField] private bool isReservedForHousing = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    private Coroutine actionRoutine;
    private Coroutine destroyRoutine;
    private StructureBuildable owningTerminal;
    private GameObject currentHousePrefab;

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
    public bool TryUpgradeHouse()
    {
        int upgradeIndex = houseLevelVal;

        if (upgradeIndex >= houseLevels.Count)
        {
            Debug.Log("[StructureWorkersHouse] House is already at max level!");
            return false;
        }

        StructureDataSO upgradeData = houseLevels[upgradeIndex];

        if (upgradeData == null || upgradeData.StructurePrefab == null)
        {
            Debug.LogWarning(
                "[StructureWorkersHouse] Upgrade data or prefab is missing!",
                this
            );
            return false;
        }

        ResourceManager resourceMgr = GameManager.Instance.ResourceManager;

        if (resourceMgr == null)
            return false;

        foreach (ResourceCostEntry cost in upgradeData.Costs)
        {
            if (cost.resourceDataSo == null || cost.amount < 0)
                return false;

            if (resourceMgr.GetResourceAmount(cost.resourceDataSo) < cost.amount)
            {
                Debug.Log(
                    $"[StructureWorkersHouse] Not enough " +
                    $"{cost.resourceDataSo.ResourceName} to upgrade!",
                    this
                );
                return false;
            }
        }

        // NEW: Preserve the old house's transform before replacing it.
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;
        Transform oldParent = transform.parent;
        Vector3 oldScale = transform.localScale;

        // NEW: Instantiate the prefab specified by the upgrade StructureDataSO.
        GameObject upgradedObject = Instantiate(
            upgradeData.StructurePrefab,
            spawnPosition,
            spawnRotation,
            oldParent
        );

        upgradedObject.transform.localScale = oldScale;

        // NEW: Validate that the upgraded prefab has the required component.
        StructureWorkersHouse upgradedHouse =
            upgradedObject.GetComponent<StructureWorkersHouse>();

        if (upgradedHouse == null)
        {
            Debug.LogError(
                "[StructureWorkersHouse] The upgraded prefab needs a StructureWorkersHouse component!",
                upgradedObject
            );

            Destroy(upgradedObject);
            return false;
        }

        // NEW: Transfer ownership and update the stored prefab.
        upgradedHouse.SetOwningTerminal(owningTerminal);
        upgradedHouse.SetCurrentPrefab(upgradeData.StructurePrefab);
        upgradedHouse.SetHouseLevel(houseLevelVal + 1);

        // NEW: Consume the upgrade costs after the replacement is validated.
        foreach (ResourceCostEntry cost in upgradeData.Costs)
        {
            if (cost.amount > 0 &&
                !resourceMgr.ConsumeResource(cost.resourceDataSo, cost.amount))
            {
                Debug.LogError(
                    "[StructureWorkersHouse] Failed to consume upgrade costs.",
                    this
                );

                Destroy(upgradedObject);
                return false;
            }
        }

        UiWorkersHouse houseUI = GameManager.Instance.uiController.UiWorkersHouse;

        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            houseUI.Setup(upgradedHouse);
        }

        // NEW: Destroy the old house only after the upgrade succeeds.
        Destroy(gameObject);

        return true;
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
                workerMgr.RecruitNewWorker();
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

    public void SetCurrentPrefab(GameObject prefab)
    {
        currentHousePrefab = prefab;
    }
    public void SetHouseLevel(int level)
    {
        houseLevelVal = level;
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
    public int HouseLevelVal => houseLevelVal;
    public List<StructureDataSO> HouseLevels => houseLevels;
}