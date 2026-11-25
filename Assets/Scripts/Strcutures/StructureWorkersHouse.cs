using System.Collections;
using UnityEngine;

public class StructureWorkersHouse : Structures
{
    [Header("House Info")]
    [SerializeField] private string structureTitle = "Worker's House";
    [SerializeField] private float destructionDuration = 3f;

    [Header("Housing Capacity & Local Workers")]
    [SerializeField] private int housingCapacity = 3; 
    [SerializeField] private int currentWorkers = 1; // 1 for Level 1, 2 for Level 2, 3 for Level 3

    [Header("Debug Status")]
    [SerializeField] private bool isDestroying = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    public int HousingCapacity => housingCapacity;
    public int CurrentWorkers => currentWorkers;

    private Coroutine destroyRoutine;
    private bool hasFlooded = false;
    private bool isRegisteredWithManager = false;
    private bool hasUnregistered = false;

    private void Start()
    {
        StartCoroutine(RegisterWhenReadyRoutine());
    }

    private IEnumerator RegisterWhenReadyRoutine()
    {
        while (GameManager.Instance == null || GameManager.Instance.WorkerManager == null)
        {
            yield return null;
        }

        if (!isRegisteredWithManager)
        {
            GameManager.Instance.WorkerManager.RegisterWorkerHouse(this);
            GameManager.Instance.WorkerManager.RegisterHouse(housingCapacity, currentWorkers);
            isRegisteredWithManager = true;
            hasUnregistered = false;
        }
    }

    private void OnDisable()
    {
        UnregisterFromManager();
    }

    private void UnregisterFromManager()
    {
        if (isRegisteredWithManager && !hasUnregistered && GameManager.Instance != null && GameManager.Instance.WorkerManager != null)
        {
            GameManager.Instance.WorkerManager.UnregisterWorkerHouse(this);
            GameManager.Instance.WorkerManager.UnregisterHouse(housingCapacity, currentWorkers);
            hasUnregistered = true;
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
        if (hasFlooded) return;

        hasFlooded = true;

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            GameManager.Instance.uiController.CloseAllModals();
        }

        if (endGameStatsManager != null)
        {
            endGameStatsManager.WorkersDied(currentWorkers);
        }

        ExecuteFinalDestruction();
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
        UnregisterFromManager();

        var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
        if (houseUI != null && houseUI.IsShowingHouse(this))
        {
            houseUI.CompleteProgress();
            GameManager.Instance.uiController.CloseAllModals();
        }

        Destroyed(); // Calls base.Destroyed() which re-enables owningTerminal and spawns BuildablePrefab!
    }

    public bool IsOccupied => currentWorkers > 0;
    public bool IsDestroying => isDestroying;
    public float ProgressPercentage => progressPercentage;
    public string StructureTitle => structureTitle;
    public string ProgressDescription => isDestroying ? "Destroying House..." : "";
}