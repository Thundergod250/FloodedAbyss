using System.Collections;
using UnityEngine;

public class StructureWorkersHouse : Structures
{
    [Header("House Info")]
    [SerializeField] private string structureTitle = "Worker's House";
    [SerializeField] private float destructionDuration = 3f;

    [Header("Debug Status")]
    [SerializeField] private bool isDestroying = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    [Header("Housing Capacity & Local Workers")]
    [SerializeField] private int housingCapacity = 1;
    [SerializeField] private int currentWorkers = 1; // Starts with 1 worker upon build

    public int HousingCapacity => housingCapacity;
    public int CurrentWorkers => currentWorkers;
    
    private Coroutine destroyRoutine;
    private bool isRegisteredWithManager = false;

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.WorkerManager != null && !isRegisteredWithManager)
        {
            GameManager.Instance.WorkerManager.RegisterWorkerHouse(this);
            // Report initial building creation and workers to WorkerManager
            GameManager.Instance.WorkerManager.RegisterHouse(housingCapacity, currentWorkers);
            isRegisteredWithManager = true;
        }
    }

    private void OnDisable()
    {
        UnregisterFromManager();
    }

    private void UnregisterFromManager()
    {
        if (isRegisteredWithManager && GameManager.Instance != null && GameManager.Instance.WorkerManager != null)
        {
            GameManager.Instance.WorkerManager.UnregisterWorkerHouse(this);
            isRegisteredWithManager = false;
        }
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
        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            GameManager.Instance.uiController.CloseAllModals();
        }

        ExecuteFinalDestruction();
    }

    public void AddWorkerCapacity(int amount = 1)
    {
        housingCapacity += amount;
        currentWorkers += amount; // Instant +1 worker on upgrade

        if (GameManager.Instance != null && GameManager.Instance.WorkerManager != null)
        {
            GameManager.Instance.WorkerManager.UpdateHouseCapacityAndWorkers(amount, amount);
        }
    }

    public void RemoveWorkerDueToDeath()
    {
        if (currentWorkers > 0)
        {
            currentWorkers--;
        }
    }

    public void ToggleDestroying()
    {
        if (isDestroying || isSubmerged) CancelDestroying();
        else StartDestroying();
    }

    private void StartDestroying()
    {
        if (isDestroying || isSubmerged) return;

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
        // When house is destroyed, all workers inside die and get reduced in WorkerManager
        if (GameManager.Instance != null && GameManager.Instance.WorkerManager != null && isRegisteredWithManager)
        {
            GameManager.Instance.WorkerManager.UnregisterHouse(housingCapacity, currentWorkers);
        }
        UnregisterFromManager();

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            houseUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        Destroyed();
    }

    public bool IsOccupied => currentWorkers > 0;
    public bool IsDestroying => isDestroying;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public string ProgressDescription => isDestroying ? "Destroying House..." : "";
}