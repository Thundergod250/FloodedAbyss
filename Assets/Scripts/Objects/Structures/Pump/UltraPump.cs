using Mono.Cecil;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class UltraPump : Item
{
    [Header("Pump Settings")]
    [SerializeField] private List<Pump> waterPump = new();

    [Header("UI")]
    [SerializeField] private GameObject ultraPumpUiGO;

    [Header("Pump Reduction Caps")]
    [SerializeField]
    private List<float> pumpReductionCaps = new()
    {
        10f,  
        30f,  
        60f,  
        100f  
    };

    public IReadOnlyList<Pump> WaterPump => waterPump;

    private void Start()
    {
        if (ultraPumpUiGO != null)
        {
            ultraPumpUiGO.SetActive(false);
        }

        foreach (Pump pump in waterPump)
        {
            pump.ultraPump = this;
        }
    }

    public override void Activate()
    {
        ultraPumpUiGO.SetActive(true);
        ultraPumpUiGO.GetComponent<UltraPumpUI>().SetUpPumpUI();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdatePumpWaterLevel()
    {
        WaterForecast waterForecast =
            GameManager.Instance.uiController
                .GetComponentInChildren<WaterForecast>();

        if (waterForecast == null)
            return;

        waterForecast.EnableTimeline();

        int activePumpCount = 0;
        float totalReduction = 0f;

        foreach (Pump pump in waterPump)
        {
            if (pump.HasResources && !pump.IsActive)
            {
                pump.SetActive(true);
            }

            if (!pump.IsActive)
                continue;

            activePumpCount++;
            totalReduction += pump.WaterAdjustment;
        }

        float reductionCap = GetReductionCap(activePumpCount);

        totalReduction = Mathf.Min(
            totalReduction,
            reductionCap
        );

        Debug.Log(
            $"Active Pumps: {activePumpCount} | " +
            $"Total Reduction: {totalReduction} | " +
            $"Cap: {reductionCap}"
        );

        waterForecast.SetPumpReduction(totalReduction);
    }

    public void PumpBecameInactive(Pump pump)
    {
        if (pump == null)
            return;

        WaterForecast waterForecast =
            GameManager.Instance.uiController
                .GetComponentInChildren<WaterForecast>();

        if (waterForecast == null)
            return;

        int activePumpCount = 0;
        float totalReduction = 0f;

        foreach (Pump activePump in waterPump)
        {
            if (!activePump.IsActive)
                continue;

            activePumpCount++;
            totalReduction += activePump.WaterAdjustment;
        }

        float reductionCap = GetReductionCap(activePumpCount);

        totalReduction = Mathf.Min(
            totalReduction,
            reductionCap
        );

        Debug.Log(
            $"Pump inactive: {pump.name} | " +
            $"Active Pumps: {activePumpCount} | " +
            $"Total Reduction: {totalReduction} | " +
            $"Cap: {reductionCap}"
        );

        waterForecast.SetPumpReduction(totalReduction);
    }

    private float GetReductionCap(int activePumpCount)
    {
        if (activePumpCount <= 0)
            return 0f;

        int index = activePumpCount - 1;

        if (index >= pumpReductionCaps.Count)
            return pumpReductionCaps[pumpReductionCaps.Count - 1];

        return pumpReductionCaps[index];
    }
}
