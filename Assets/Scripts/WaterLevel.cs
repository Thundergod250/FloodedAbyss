using UnityEngine;

public class WaterLevel : MonoBehaviour
{
    [Header("Ocean Visual Reference")]
    [SerializeField] private Transform oceanTransform; // Assign your Ocean GameObject here

    [Header("Water Height Settings")]
    [SerializeField] private float maxWaterHeight = 300f;     
    [SerializeField] private float minWaterHeight = 0f;       

    [Header("Water Pressure Progression")]
    [SerializeField] private float pressureIncreaseInterval = 10f; // How many seconds before Water Pressure increments by 1
    [SerializeField] private int baseWaterPressure = 1;            // Starting water pressure value

    [Header("Pump Mechanics")]
    private float baseWaterHeight = 100f;                          // Captured starting baseline height
    private float netDischargePower = 0f;
    private float pressureTimer = 0f;
    private int currentWaterPressure;
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
        currentWaterPressure = baseWaterPressure;
    }

    private void Update()
    {
        // 1. Water Pressure constantly ticks up independently over time (1, 2, 3, 4, 5...)
        pressureTimer += Time.deltaTime;
        if (pressureTimer >= pressureIncreaseInterval)
        {
            pressureTimer = 0f;
            currentWaterPressure++;
            Debug.Log($"[WaterLevel] Water Pressure increased to: {currentWaterPressure}");
        }

        // 2. New Water Height = Base Starting Water Height + Current Water Pressure (minus discharge mitigation)
        float targetHeight = baseWaterHeight + currentWaterPressure - netDischargePower;
        currentWaterHeight = Mathf.Clamp(targetHeight, minWaterHeight, maxWaterHeight);

        // Physically move the ocean object up or down based on the new height
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
    
    // Water Pressure is its own independent integer progression value displayed on the red HUD text
    public int WaterPressure => currentWaterPressure;
}