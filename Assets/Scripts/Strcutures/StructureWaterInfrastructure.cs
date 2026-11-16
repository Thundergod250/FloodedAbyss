using System.Collections;
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

    [Header("Water Pressure Reduction")]
    [SerializeField] private float waterReduc = 1f;

    [Header("Worker Requirements")]
    [SerializeField] private bool requiresWorkers = false;
    [SerializeField] private int laborCost = 1;

    [Header("Flood Gate Settings")]
    [SerializeField] private bool requiresManualActivation = false;

    [Header("Debug Status")]
    [SerializeField] private bool isDestroying = false;

    private float appliedWaterReduction;
    private bool isActivated;

    public string StructureName => structureName;
    public int LaborCost => laborCost;

    public bool GreatFloodActive =>
        GameManager.Instance != null &&
        GameManager.Instance.WaterLevel != null &&
        GameManager.Instance.WaterLevel.GreatFloodStarted;

    private float GetWeatherBasedWaterReduction()
    {
        WaterForecast wl = GameManager.Instance.uiController
            .GetComponent<WaterForecast>();

        float weatherValue = wl.CurrentActiveWeather switch
        {
            WeatherType.Sunny => wl.SunnyVal,
            WeatherType.Drizzle => wl.DrizzleVal,
            WeatherType.Stormy => wl.StormyVal,
            WeatherType.Typhoon => wl.TyphoonVal,
            _ => 1f
        };

        return weatherValue * waterReduc;
    }

    private UiWaterInfrastucture UIWaterInfrastructure()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.uiController == null)
            return null;

        return GameManager.Instance.uiController.UIWaterInfrastructure;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Activate()
    {
        var waterInfrastructureUI = UIWaterInfrastructure();

        if (waterInfrastructureUI == null)
        {
            Debug.LogWarning(
                "[StructureWaterInfrastructure] UiWaterInfrastructure reference not found!"
            );
            return;
        }

        waterInfrastructureUI.Setup(this);
        GameManager.Instance.uiController.OpenModal<UiWaterInfrastucture>();
    }

    public void ReduceWaterPressure()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.WaterLevel == null)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;
        var waterS = UIWaterInfrastructure();

        if (isActivated)
            return;

        if (requiresWorkers)
        {
            if (workerMgr == null || !workerMgr.TryConsumeWorkersForLabor(laborCost))
            {
                waterS?.ShowNotEnoughWorkersWarning();
                return;
            }
            workerMgr.RegisterActiveWorkplace(this);
        }

        appliedWaterReduction = GetWeatherBasedWaterReduction();

        float reduction = GreatFloodActive
            ? appliedWaterReduction * 0.8f
            : appliedWaterReduction;

        GameManager.Instance.WaterLevel.ReduceWaterPressure(reduction);

        isActivated = true;

        Debug.Log(
            $"{name} activated. Water pressure reduction: {reduction}"
        );
    }

    public void IncWaterPressure()
    {
        if (!isActivated)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;

        GameManager.Instance.WaterLevel.IncWaterPressure(
            appliedWaterReduction
        );

        if (requiresWorkers && workerMgr != null)
        {
            workerMgr.UnregisterActiveWorkplace(this);
            workerMgr.ReturnWorkersFromLabor(laborCost);
        }

        appliedWaterReduction = 0f;
        isActivated = false;
    }

    public void ForceStopByWorkerShortage()
    {
        if (!isActivated)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;

        GameManager.Instance.WaterLevel.IncWaterPressure(
            appliedWaterReduction
        );

        if (requiresWorkers && workerMgr != null)
        {
            workerMgr.UnregisterActiveWorkplace(this);
        }

        appliedWaterReduction = 0f;
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
}