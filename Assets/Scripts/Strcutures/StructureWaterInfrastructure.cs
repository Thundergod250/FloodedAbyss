using UnityEngine;

public class StructureWaterInfrastructure : Structures, IWorkerUser
{
    public enum InfrastructureType
    {
        WaterStorage,
        EarthenDike,
        FloodGates,
        ConcreteRiverWall
    }

    [Header("Infrastructure Settings")]
    [SerializeField] private InfrastructureType infrastructureType;
    [SerializeField] private string structureName;

    [Header("Worker Requirements")]
    [SerializeField] private bool requiresWorkers = false;
    [SerializeField] private int laborCost = 1;

    [Header("Flood Gate Settings")]
    [SerializeField] private bool requiresManualActivation = false;

    [Header("Water Reduction Settings")]
    [SerializeField] private float reductionMultiplier = 1f;

    [Header("Debug Status")]
    [SerializeField] private bool isDestroying = false;

    private bool isActivated;

    public string StructureName => structureName;
    public int LaborCost => laborCost;

    private UiWaterInfrastucture UIWaterInfrastructure()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.uiController == null)
            return null;

        return GameManager.Instance.uiController.UIWaterInfrastructure;
    }

    public override void Activate()
    {
        var waterInfrastructureUI = UIWaterInfrastructure();

        if (waterInfrastructureUI == null)
        {
            Debug.LogWarning(
                "[StructureWaterInfrastructure] UI reference not found!"
            );
            return;
        }

        waterInfrastructureUI.Setup(this);
        GameManager.Instance.uiController.OpenModal<UiWaterInfrastucture>();

        if(!requiresWorkers) // if doesn't need workers
        {
            GameManager.Instance.uiController.GetComponent<UiWaterInfrastucture>().HideActivateButton();
        }
    }

    private void Start()
    {
        if (!requiresWorkers)
        {
            ReduceWaterPressure();
        }
    }

    public void ReduceWaterPressure()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.WaterLevel == null)
            return;

        if (isActivated)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var waterS = UIWaterInfrastructure();

        if (requiresWorkers)
        {
            if (workerMgr == null ||
                !workerMgr.TryConsumeWorkersForLabor(laborCost))
            {
                waterS?.ShowNotEnoughWorkersWarning();
                return;
            }

            workerMgr.RegisterActiveWorkplace(this);
        }

        isActivated = true;

        // Register this active infrastructure.
        GameManager.Instance.WaterLevel.RegisterInfrastructure(this);
    }

    public void IncWaterPressure()
    {
        if (!isActivated)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;

        // Remove this infrastructure's reduction.
        GameManager.Instance.WaterLevel.UnregisterInfrastructure(this);

        if (requiresWorkers && workerMgr != null)
        {
            workerMgr.UnregisterActiveWorkplace(this);
            workerMgr.ReturnWorkersFromLabor(laborCost);
        }

        isActivated = false;
    }

    public void ForceStopByWorkerShortage()
    {
        if (!isActivated)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;

        // Remove this infrastructure's reduction.
        GameManager.Instance.WaterLevel.UnregisterInfrastructure(this);

        if (requiresWorkers && workerMgr != null)
        {
            workerMgr.UnregisterActiveWorkplace(this);
        }

        isActivated = false;
    }

    // Activation is permanent once activated.
    public void ToggleActivation()
    {
        if (!isActivated)
            ReduceWaterPressure();
    }

    public void DestroyStructure()
    {
        if (isDestroying)
            return;

        isDestroying = true;

        if (isActivated)
            IncWaterPressure();

        Destroyed();
    }

    public bool IsActivated => isActivated;
    public bool IsDestroying => isDestroying;
    public bool RequiresWorkers => requiresWorkers;
    public InfrastructureType Type => infrastructureType;
    public bool RequiresManualActivation => requiresManualActivation; 
    public float ReductionMultiplier => reductionMultiplier;
}