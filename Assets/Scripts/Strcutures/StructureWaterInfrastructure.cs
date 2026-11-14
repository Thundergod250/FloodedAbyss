using System.Collections;
using UnityEngine;

public class StructureWaterInfrastructure : Structures
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

    [Header("Durability")]
    [SerializeField] private bool canStoreWater = true;
    [SerializeField] private float waterRemoveDuration = 5f;
    [SerializeField] private float waterStored;
    [SerializeField] private float maxWaterStored = 100f;
    [SerializeField] private float waterPerSecInc;

    [Header("Great Flood Durability Damage")]
    [SerializeField] private bool damageDuringGreatFlood = true;
    [SerializeField] private float greatFloodDamagePerSecond = 1f;

    [Header("Flood Gate Settings")]
    [SerializeField] private bool requiresManualActivation = false;

    [Header("Debug Status")]
    [SerializeField] private bool isWorking = false;
    [SerializeField] private bool isDestroying = false;
    [SerializeField, Range(0f, 1f)] private float progressPercentage = 0f;

    [Header("Great Flood Status")]
    [SerializeField] private bool greatFloodActive;


    private float GetWeatherBasedWaterReduction()
    {
        WaterForecast wl = GameManager.Instance.uiController.GetComponent<WaterForecast>();

        float weatherValue = wl.CurrentActiveWeather switch
        {
            WeatherType.Sunny => wl.SunnyVal,
            WeatherType.Drizzle => wl.DrizzleVal,
            WeatherType.Stormy => wl.StormyVal,
            WeatherType.Typhoon => wl.TyphoonVal,
            _ => 1f
        };

        return weatherValue * waterReduc * wl.GetWeatherMultiplier();
    }

    private float appliedWaterReduction;
    private float currentDurability;
    private bool isActivated;
    private bool needToDump;

    public string StructureName => structureName;
    public int LaborCost => laborCost;

    private UiWaterInfrastucture UIWaterInfrastructure()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.uiController == null)
        {
            return null;
        }

        return GameManager.Instance.uiController.UIWaterInfrastructure;
    }

    private void Start()
    {
        waterStored = Mathf.Clamp(waterStored, 0f, maxWaterStored);
        greatFloodActive = false;
    }

    protected override void Update()
    {
        base.Update();

        if (isActivated && canStoreWater)
        {
            waterStored = Mathf.Min(
                waterStored + (waterPerSecInc * Time.deltaTime),
                maxWaterStored
            );
        }

        if (waterStored >= maxWaterStored && isActivated)
        {
            ToggleActivation();
            needToDump = true;
        }

        if (GameManager.Instance.WaterLevel.GreatFloodStarted)
        {
            waterStored = Mathf.Min(waterStored + (10f * Time.deltaTime), maxWaterStored);
        }
    }

    public override void Activate()
    {
        var waterInfrastructureUI = UIWaterInfrastructure();

        if (waterInfrastructureUI == null)
        {
            Debug.LogWarning("[StructureFarm] UiFarm reference not found!");
            return;
        }

        waterInfrastructureUI.Setup(this);
        GameManager.Instance.uiController.OpenModal<UiWaterInfrastucture>();
    }

    public void TakeDamage(float damage)
    {
        if (!canStoreWater || damage <= 0f) return;

        currentDurability -= damage;

        Debug.Log($"{name} durability: {currentDurability}/{waterStored}");

        if (currentDurability <= 0f)
        {
            Destroyed();
        }
    }

    public void ReduceWaterPressure()
    {
        var workerMgr = GameManager.Instance.WorkerManager;
        var waterS = UIWaterInfrastructure();

        if (workerMgr == null || isActivated || needToDump)
            return;

        if (requiresWorkers && workerMgr.CurrentWorker < laborCost)
        {
            waterS?.ShowNotEnoughWorkersWarning();
            return;
        }

        if (requiresWorkers)
            workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker - laborCost);

        appliedWaterReduction = GetWeatherBasedWaterReduction();

        if (!greatFloodActive)
        {
            GameManager.Instance.WaterLevel.ReduceWaterPressure(appliedWaterReduction);
        }
        else
        {
            GameManager.Instance.WaterLevel.ReduceWaterPressure(appliedWaterReduction + 0.2f);
        }

        isActivated = true;

        Debug.Log($"{name} activated. Weather-based water pressure reduction: {appliedWaterReduction}");
    }

    private IEnumerator WorkLoopRoutine()
    {
        isWorking = true;
        isDestroying = false;

        float startingWater = waterStored;
        float elapsed = 0f;

        while (elapsed < waterRemoveDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / waterRemoveDuration);

            waterStored = Mathf.Lerp(startingWater, 0f, progress);
            progressPercentage = progress;

            yield return null;
        }

        waterStored = 0f;
        progressPercentage = 1f;

        needToDump = false;
        isWorking = false;
    }

    public void IncWaterPressure()
    {
        if (!isActivated)
            return;

        var workerMgr = GameManager.Instance.WorkerManager;

        GameManager.Instance.WaterLevel.IncWaterPressure(appliedWaterReduction);

        if (requiresWorkers && workerMgr != null)
            workerMgr.SetCurrentWorkers(workerMgr.CurrentWorker + laborCost);

        appliedWaterReduction = 0f;
        isActivated = false;
    }

    public void ToggleActivation()
    {
        if (isActivated)
            IncWaterPressure();
        else
            ReduceWaterPressure();
    }

    public void StartWaterRemoval()
    {
        if (!needToDump || isWorking)
            return;

        StartCoroutine(WorkLoopRoutine());
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

    public float CurrentDurability => currentDurability;
    public float WaterStored => waterStored;
    public bool CanStoreWater => canStoreWater;
    public bool IsActivated => isActivated;
    public bool IsWorking => isWorking;
    public bool IsDestroying => isDestroying;
    public bool RequiresWorkers => requiresWorkers;
    public InfrastructureType Type => infrastructureType;
    public bool CanEmptyStorage =>
        canStoreWater && needToDump && waterStored > 0f && !isWorking;
}
