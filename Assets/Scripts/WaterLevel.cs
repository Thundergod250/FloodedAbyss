using UnityEngine;

public class WaterLevel : MonoBehaviour
{
    [Header("Ocean Visual Reference")]
    [SerializeField] private Transform oceanTransform; // Assign your Ocean GameObject here (or leave null if this script is on the ocean itself)

    [Header("Water Height Settings")]
    [SerializeField] private float currentWaterHeight = 100f; // Starting height
    [SerializeField] private float maxWaterHeight = 200f;     // Maximum cap
    [SerializeField] private float minWaterHeight = 0f;       // Minimum cap

    [Header("Water Pressure Settings")]
    [SerializeField] private float waterPressureRate = 1f;    // The constant rate at which water pressure / rising occurs (e.g. 1 m/s)

    private float startingWaterLevel;
    private float netDischargePower = 0f;

    private void Awake()
    {
        // If no transform is explicitly assigned, assume this script is attached directly to the Ocean object
        if (oceanTransform == null)
        {
            oceanTransform = transform;
        }
    }

    private void Start()
    {
        // Capture the initial water level at the very start of the game
        startingWaterLevel = currentWaterHeight;
    }

    private void Update()
    {
        // Water constantly rises based on the water pressure rate minus active pump discharge power
        float heightDelta = (waterPressureRate - netDischargePower) * Time.deltaTime;
        
        currentWaterHeight = Mathf.Clamp(currentWaterHeight + heightDelta, minWaterHeight, maxWaterHeight);

        // Physically move the ocean object up or down based on current height
        if (oceanTransform != null)
        {
            Vector3 pos = oceanTransform.position;
            pos.y = currentWaterHeight;
            oceanTransform.position = pos;
        }
    }
    
    public void ModifyDischargePower(float amount)
    {
        netDischargePower += amount;
        if (netDischargePower < 0f) netDischargePower = 0f;
    }

    public void SetDischargePower(float amount)
    {
        netDischargePower = Mathf.Max(0f, amount);
    }

    public float CurrentWaterHeight => currentWaterHeight;
    public float NetDischargePower => netDischargePower;
    
    // Returns rounded integer difference for clean UI display without decimals
    public int WaterPressure => Mathf.RoundToInt(currentWaterHeight - startingWaterLevel);
}