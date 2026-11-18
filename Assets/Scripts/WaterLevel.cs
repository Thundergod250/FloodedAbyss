using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class WaterLevel : MonoBehaviour
{
    [Header("Ocean Visual Reference")]
    [SerializeField] private Transform oceanTransform; // Assign your Ocean GameObject here

    [Header("Water Height Settings")]
    [SerializeField] private float maxWaterHeight = 300f;     
    [SerializeField] private float minWaterHeight = 0f;       
    [SerializeField] private float drainSmoothSpeed = 10f;     // How fast the water level reacts to pumps (higher = faster draining)

    [Header("Water Pressure Progression")]
    [SerializeField] private float pressureIncreaseRate = 0.2f;    // How fast water pressure accumulates per second smoothly
    [SerializeField] private float baseWaterPressure = 1f;         // Starting water pressure value
    [SerializeField] private bool startWaterPressure = false;        // Start water pressure

    [Header("Great Flood Settings")]
    [SerializeField] private bool forceStartGreatFlood = false;
    [SerializeField] private float greatFloodPressureIncrease = 15f;
    [SerializeField] private float greatFloodMaxWaterHeightCap = 250f;
    [SerializeField] private int greatFloodTimerMins;
    [SerializeField] private int greatFloodTimerDuration;
    [SerializeField] private float greatFloodTimerProgress;

    [Header("Infrastructure Reduction")]
    [SerializeField] private float infrastructureReductionPerStructure = 60f;

    private HashSet<StructureWaterInfrastructure> activeInfrastructures =
        new HashSet<StructureWaterInfrastructure>();

    private float greatFloodDurationProgress = 0f;

    private bool greatFloodStarted = false;

    [Header("Pump Mechanics")]
    private float baseWaterHeight = 87f;                           // Captured starting baseline height
    private float netDischargePower = 0f;
    private float currentWaterPressureFloat;
    private float currentWaterHeight;

    public int GreatFloorTimerDuration => greatFloodTimerDuration;

    private void Awake()
    {
        if (oceanTransform == null)
        {
            oceanTransform = transform;
        }
        
        // Capture the baseline height from the ocean's initial position
        baseWaterHeight = oceanTransform.position.y;
        currentWaterHeight = baseWaterHeight;
        currentWaterPressureFloat = baseWaterPressure;
    }

    private void Update()
    {
        if (forceStartGreatFlood && !greatFloodStarted)
        {
            StartGreatFlood();
        }

        if (startWaterPressure)
        {
            float effectivePressureIncrease = pressureIncreaseRate;

            if (greatFloodStarted)
            {
                effectivePressureIncrease = Mathf.Max(0f, pressureIncreaseRate - (TotalInfrastructureReduction / 60f));
            }

            currentWaterPressureFloat += effectivePressureIncrease * Time.deltaTime;
        }

        // 2. Target Water Height = Base Starting Height + Water Pressure - Discharge Pressure
        float targetHeight = baseWaterHeight + currentWaterPressureFloat - netDischargePower;

        targetHeight = Mathf.Clamp(targetHeight, minWaterHeight, maxWaterHeight);
        // 3. Smoothly interpolate (Lerp) current water height toward the target height.
        // This makes drainage fast and responsive when pumps turn on, but eliminates instant snapping.
        // Great Flood: Rise at the same rate as greatFloodPressureIncrease
        if (greatFloodStarted)
        {
            targetHeight = Mathf.Min(greatFloodMaxWaterHeightCap, currentWaterHeight + greatFloodPressureIncrease * Time.deltaTime);
        }

        currentWaterHeight = Mathf.Lerp(
            currentWaterHeight,
            targetHeight,
            drainSmoothSpeed * Time.deltaTime
        );

        if (oceanTransform != null)
        {
            Vector3 pos = oceanTransform.position;
            pos.y = currentWaterHeight;
            oceanTransform.position = pos;
        }

    }
    
    public void SetDischargePower(float amount)
    {
        netDischargePower = Mathf.Max(0f, amount);
    }

    public void SetOceanHeight(int amount)
    {
        maxWaterHeight = Mathf.Max(0f, amount);
    }

    public void ReduceWaterPressure(float amount)
    {
        pressureIncreaseRate = Mathf.Max(0f, pressureIncreaseRate - Mathf.Max(0f, amount));
    }

    public void IncWaterPressure(float amount)
    {
        pressureIncreaseRate += amount;
    }

    public IEnumerator GreatFloodTimer()
    {
        float maxTime = greatFloodTimerMins * 60f;
        greatFloodTimerProgress = 0f;

        while (greatFloodTimerProgress < maxTime)
        {
            greatFloodTimerProgress += Time.deltaTime;
            yield return null;
        }

        StartGreatFlood();
    }

    public void StartGreatFlood()
    {
        if (greatFloodStarted) return;

        greatFloodStarted = true;
        greatFloodDurationProgress = 0f;

        pressureIncreaseRate += greatFloodPressureIncrease;
        GreatFloodCompleted = false;

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            WaterForecast forecast = GameManager.Instance.uiController.GetComponentInChildren<WaterForecast>();

            if (forecast != null)
                forecast.StopWeather();
        }

        GameManager.Instance.VFXController.PlayRain(rainStrength.Strong);

        StartWaterRise(true);

        Debug.Log("GREAT FLOOD STARTED!");

        StartCoroutine(GreatFloodDurationTimer());
    }

    private IEnumerator GreatFloodDurationTimer()
    {
        float floodDuration = greatFloodTimerDuration * 60f;

        while (greatFloodDurationProgress < floodDuration)
        {
            greatFloodDurationProgress += Time.deltaTime;
            yield return null;
        }

        EndGreatFlood();
    }

    public void RegisterInfrastructure(StructureWaterInfrastructure structure)
    {
        if (structure != null)
            activeInfrastructures.Add(structure);
    }

    public void UnregisterInfrastructure(StructureWaterInfrastructure structure)
    {
        if (structure != null)
            activeInfrastructures.Remove(structure);
    }

    public float TotalInfrastructureReduction
    {
        get
        {
            float reduction = 0f;

            foreach (var infrastructure in activeInfrastructures)
            {
                if (infrastructure != null)
                {
                    reduction += infrastructureReductionPerStructure *
                                 infrastructure.ReductionMultiplier;
                }
            }

            float effectiveness = greatFloodStarted ? 0.8f : 1f;

            return reduction * effectiveness;
        }
    }

    public float GetAdjustedWeatherTarget(float weatherAmount)
    {
        return Mathf.Max(0f, weatherAmount - TotalInfrastructureReduction);
    }
    public void AdjustWaterLevel(float amount)
    {
        currentWaterPressureFloat = Mathf.Max(0f,currentWaterPressureFloat + amount);
    }

    public void EndGreatFlood()
    {
        if (!greatFloodStarted) return;

        greatFloodStarted = false;

        pressureIncreaseRate -= greatFloodPressureIncrease;
        GreatFloodCompleted = true;

        GameManager.Instance.VFXController.StopRain();

        StartWaterRise(false);

        Debug.Log("GREAT FLOOD ENDED!");
    }

    public void StartWaterRise(bool val)
    {
        startWaterPressure = val;
        if (val)
            currentWaterPressureFloat += 1;
    }

    public float CurrentWaterHeight => oceanTransform != null ? oceanTransform.position.y : currentWaterHeight;
    public float NetDischargePower => netDischargePower;
    public float TimeUntilGreatFlood => Mathf.Max(0f, (greatFloodTimerMins * 60f) - greatFloodTimerProgress);
    public float MaxTimeUntilGreatFlood => greatFloodTimerMins * 60;
    public bool GreatFloodStarted => greatFloodStarted;
    public float GreatFloodTimeRemaining => Mathf.Max(0f, (greatFloodTimerDuration * 60f) - greatFloodDurationProgress); 
    public bool GreatFloodCompleted { get; private set; } = false;
    public float MaxWaterHeight => maxWaterHeight;
    // Water Pressure casts to an integer for your red HUD text so it displays whole numbers (1, 2, 3...) 
    // while the underlying float increments smoothly.
    public int WaterPressure => Mathf.FloorToInt(currentWaterPressureFloat);
}