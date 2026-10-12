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
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    private Coroutine actionRoutine;
    private Coroutine destroyRoutine;
    private GameObject terminalPrefab;

    public void SetTerminalPrefab(GameObject prefab)
    {
        terminalPrefab = prefab;
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

    public void ToggleHouseAction()
    {
        if (isOccupied || isDestroying) return;

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
        if (isWorking || isOccupied) return;

        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr == null) return;

        // If no homeless workers, we are recruiting and need to validate resource costs
        if (workerMgr.HomelessWorker <= 0 && recruitmentCostResource != null)
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

        actionRoutine = StartCoroutine(ActionRoutine());
    }

    private void StopAction()
    {
        if (!isWorking) return;

        if (actionRoutine != null)
        {
            StopCoroutine(actionRoutine);
            actionRoutine = null;
        }

        // Refund recruitment cost if stopped mid-recruitment
        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null && workerMgr.HomelessWorker <= 0 && recruitmentCostResource != null)
        {
            GameManager.Instance.ResourceManager.AddResource(recruitmentCostResource, recruitmentCostAmount);
        }

        isWorking = false;
        progressPercentage = 0f;
    }

    private IEnumerator ActionRoutine()
    {
        isWorking = true;
        progressPercentage = 0f;
        float elapsed = 0f;

        while (elapsed < actionDuration)
        {
            if (isDestroying) yield break;
            elapsed += Time.deltaTime;
            progressPercentage = Mathf.Clamp01(elapsed / actionDuration);
            yield return null;
        }

        progressPercentage = 1f;
        ExecuteCompletion();
    }

    private void ExecuteCompletion()
    {
        var workerMgr = GameManager.Instance.WorkerManager;
        if (workerMgr != null)
        {
            if (workerMgr.HomelessWorker > 0)
            {
                workerMgr.TryHouseWorker();
            }
            else
            {
                workerMgr.RecruitNewWorker();
            }
        }

        isOccupied = true;
        isWorking = false;

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            houseUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    public void ToggleDestroying()
    {
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
        if (isDestroying) return;
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
            elapsed += Time.deltaTime;
            progressPercentage = Mathf.Clamp01(elapsed / destructionDuration);
            yield return null;
        }

        progressPercentage = 1f;
        ExecuteFinalDestruction();
    }

    private void ExecuteFinalDestruction()
    {
        // If this house was occupied, free the house and turn the worker homeless again!
        if (isOccupied)
        {
            var workerMgr = GameManager.Instance.WorkerManager;
            if (workerMgr != null)
            {
                workerMgr.FreeHouse();
            }
        }

        if (terminalPrefab != null)
        {
            Instantiate(terminalPrefab, transform.position, transform.rotation);
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
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public string ProgressDescription => isDestroying ? "Destroying House..." : (GameManager.Instance.WorkerManager.HomelessWorker > 0 ? "Housing Worker..." : "Recruiting Worker...");
}