using UnityEngine;

public class WaterLevel : MonoBehaviour
{
    [Header("Ocean Visual Reference")]
    [SerializeField] private Transform oceanTransform; // Assign your Ocean GameObject here (or leave null if this script is on the ocean itself)

    [Header("Water Height Settings")]
    [Range(0f, 200f)]
    [SerializeField] private float currentWaterHeight = 100f; // Starting height
    [SerializeField] private float riseRate = 1.5f;             // Base rate at which water constantly rises per second
    [SerializeField] private float maxWaterHeight = 200f;     // Maximum cap
    [SerializeField] private float minWaterHeight = 0f;       // Minimum cap

    [Header("Water Height ")]
    private float netDischargePower = 0f;

    private void Awake()
    {
        // If no transform is explicitly assigned, assume this script is attached directly to the Ocean object
        if (oceanTransform == null)
        {
            oceanTransform = transform;
        }
    }

    private void Update()
    {
        // Calculate net change: constant rising minus active pump discharge power
        float heightDelta = (riseRate - netDischargePower) * Time.deltaTime;
        
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
    public Transform OceanTransform => oceanTransform;

}