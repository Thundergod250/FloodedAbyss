using System.Collections;
using UnityEngine;

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
    [SerializeField] private bool startWaterPressure;        // Start water pressure

    [Header("Great Flood Settings")]
    [SerializeField] private bool forceStartGreatFlood = false;
    [SerializeField] private float greatFloodPressureIncrease = 15f;
    [SerializeField] private int greatFloodTimer;
    [SerializeField] private float greatFloodTimerProgress;

    private bool greatFloodStarted = false;

    [Header("Pump Mechanics")]
    private float baseWaterHeight = 87f;                           // Captured starting baseline height
    private float netDischargePower = 0f;
    private float currentWaterPressureFloat;
    private float currentWaterHeight;

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

    private void Start()
    {
        if (!forceStartGreatFlood)
        {
          // StartCoroutine(GreatFloodTimer());
        }
    }

    private void Update()
    {
        if (forceStartGreatFlood && !greatFloodStarted)
        {
            StartGreatFlood();
        }

        // 1. Water Pressure accumulates smoothly over time as a float rate
        currentWaterPressureFloat += pressureIncreaseRate * Time.deltaTime;

        // 2. Target Water Height = Base Starting Height + Water Pressure - Discharge Pressure
        float targetHeight = baseWaterHeight + currentWaterPressureFloat - netDischargePower;
        targetHeight = Mathf.Clamp(targetHeight, minWaterHeight, maxWaterHeight);

        // 3. Smoothly interpolate (Lerp) current water height toward the target height.
        // This makes drainage fast and responsive when pumps turn on, but eliminates instant snapping.
        currentWaterHeight = Mathf.Lerp(currentWaterHeight, targetHeight, drainSmoothSpeed * Time.deltaTime);

        // Physically move the ocean object smoothly based on the interpolated height
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
        pressureIncreaseRate -= Mathf.Max(0f, pressureIncreaseRate - amount);
    }

    public void IncWaterPressure(float amount)
    {
        pressureIncreaseRate += amount;
    }

    public IEnumerator GreatFloodTimer()
    {
        float maxTime = greatFloodTimer * 60f;
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
        pressureIncreaseRate += greatFloodPressureIncrease;

        Debug.Log("GREAT FLOOD STARTED!");
    }

    public void StartWaterRise(bool val)
    {
        startWaterPressure = val;
        currentWaterPressureFloat += 1;
    }

    public float CurrentWaterHeight => oceanTransform != null ? oceanTransform.position.y : currentWaterHeight;
    public float NetDischargePower => netDischargePower;
    public float TimeUntilGreatFlood => greatFloodTimerProgress;
    public float MaxTimeUntilGreatFlood => greatFloodTimer * 60;
    public bool GreatFloodStarted => greatFloodStarted;
    
    // Water Pressure casts to an integer for your red HUD text so it displays whole numbers (1, 2, 3...) 
    // while the underlying float increments smoothly.
    public int WaterPressure => Mathf.FloorToInt(currentWaterPressureFloat);
}