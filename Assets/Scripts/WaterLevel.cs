using UnityEngine;

public class WaterLevel : MonoBehaviour
{
    [Header("Ocean Visual Reference")]
    [SerializeField] private Transform oceanTransform; // Assign your Ocean GameObject here

    [Header("Water Height Settings")]
    [SerializeField] private float maxWaterHeight = 300f;     
    [SerializeField] private float minWaterHeight = 0f;       

    [Header("Water Pressure Progression")]
    [SerializeField] private float pressureIncreaseRate = 0.2f;    // How fast water pressure accumulates per second smoothly
    [SerializeField] private float baseWaterPressure = 1f;         // Starting water pressure value

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
        currentWaterPressureFloat = baseWaterPressure;
    }

    private void Update()
    {
        // 1. Water Pressure accumulates smoothly over time as a float rate
        currentWaterPressureFloat += pressureIncreaseRate * Time.deltaTime;

        // 2. Direct Calculation: Water Height = Base Starting Water Height + Current Pressure Float 
        // (Discharge power is excluded from this calculation for now, so pumps only show up on the UI)
        float targetHeight = baseWaterHeight + currentWaterPressureFloat;
        currentWaterHeight = Mathf.Clamp(targetHeight, minWaterHeight, maxWaterHeight);

        // Physically move the ocean object smoothly based on the calculated height
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

    public float CurrentWaterHeight => oceanTransform != null ? oceanTransform.position.y : currentWaterHeight;
    public float NetDischargePower => netDischargePower;
    
    // Water Pressure casts to an integer for your red HUD text so it displays whole numbers (1, 2, 3...) 
    // while the underlying float increments smoothly.
    public int WaterPressure => Mathf.FloorToInt(currentWaterPressureFloat);
}