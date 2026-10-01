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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UpdatePumpWaterLevel()
    {
        float totalReduction = 0f;

        foreach (Pump pump in waterPump)
        {
            if (pump.HasResources && !pump.IsActive)
            {
                totalReduction += pump.MaximumWaterReduction;

                pump.SetActive(true);
                pump.DrainResources();
            }
        }

        if (totalReduction <= 0f)
            return;

        WaterForecast waterForecast =
            GameManager.Instance.uiController
                .GetComponentInChildren<WaterForecast>();

        if (waterForecast == null)
            return;

        waterForecast.ChangeMaximumWater(-totalReduction);

        waterForecast.EnableTimeline();

        ultraPumpUiGO.SetActive(false);
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

        waterForecast.ChangeMaximumWater(pump.MaximumWaterReduction);
    }
    public void ActivatePumps()
    {
        foreach (Pump pump in waterPump)
        {
            pump.ConfirmResources();
        }

        UpdatePumpWaterLevel();
    }
}
