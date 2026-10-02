using UnityEngine;
using System.Collections.Generic;

public class UiApexPump : UiModals
{
    [Header("Resource Panels")]
    [SerializeField] private List<PumpResourcePanel> resourcePanels = new List<PumpResourcePanel>();

    [Header("Water Target Thresholds per Pump")]
    [SerializeField] private float woodTargetHeight = 78f;
    [SerializeField] private float stoneTargetHeight = 65f;
    [SerializeField] private float copperTargetHeight = 50f;
    [SerializeField] private float ironTargetHeight = 35f;
    [SerializeField] private float goldTargetHeight = 20f;

    private PlayerResources playerResources;

    protected override void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }

        if (playerResources == null)
        {
            Debug.LogError("UiApexPump: PlayerResources could not be found!");
            return;
        }

        // Initialize panels & subscribe to events
        foreach (var panel in resourcePanels)
        {
            if (panel != null)
            {
                panel.Initialize(playerResources);

                panel.OnActiveStateChanged -= HandlePanelActiveStateChanged;
                panel.OnActiveStateChanged += HandlePanelActiveStateChanged;
            }
        }

        LogAllPumpStates();
    }

    private void HandlePanelActiveStateChanged(PumpResourcePanel panel, bool isActive)
    {
        string state = isActive ? "ACTIVE" : "INACTIVE";
        Debug.Log($"[UiApexPump Notification] {panel.ResourceType} pump switched to {state}. Total Active Pumps: {GetActivePumpCount()}/{resourcePanels.Count}");

        if (isActive)
        {
            TriggerWaterDrainForResource(panel.ResourceType);
        }

        LogAllPumpStates();
    }

    private void TriggerWaterDrainForResource(ResourceType type)
    {
        WaterLevel waterLevel = GameManager.Instance != null ? GameManager.Instance.waterLevel : null;

        if (waterLevel == null)
        {
            Debug.LogWarning("UiApexPump: GameManager.Instance.waterLevel reference is missing!");
            return;
        }

        switch (type)
        {
            case ResourceType.Wood:
                waterLevel.DrainToTargetHeight(woodTargetHeight);
                break;

            case ResourceType.Stone:
                waterLevel.DrainToTargetHeight(stoneTargetHeight);
                break;

            case ResourceType.Copper:
                waterLevel.DrainToTargetHeight(copperTargetHeight);
                break;

            case ResourceType.Iron:
                waterLevel.DrainToTargetHeight(ironTargetHeight);
                break;

            case ResourceType.Gold:
                waterLevel.DrainToTargetHeight(goldTargetHeight);
                break;
        }
    }

    public int GetActivePumpCount()
    {
        int activeCount = 0;
        foreach (var panel in resourcePanels)
        {
            if (panel != null && panel.IsActive)
            {
                activeCount++;
            }
        }
        return activeCount;
    }

    public void LogAllPumpStates()
    {
        List<string> activePumps = new List<string>();
        List<string> inactivePumps = new List<string>();

        foreach (var panel in resourcePanels)
        {
            if (panel == null) continue;

            if (panel.IsActive)
            {
                activePumps.Add($"{panel.ResourceType} ({panel.CurrentEnergy})");
            }
            else
            {
                inactivePumps.Add($"{panel.ResourceType} ({panel.CurrentEnergy})");
            }
        }

        Debug.Log($"[UiApexPump Summary] Active ({activePumps.Count}): {string.Join(", ", activePumps)} | Inactive ({inactivePumps.Count}): {string.Join(", ", inactivePumps)}");
    }

    public int GetTotalEnergy()
    {
        int total = 0;
        foreach (var panel in resourcePanels)
        {
            if (panel != null) total += panel.CurrentEnergy;
        }
        return total;
    }

    private void OnDestroy()
    {
        foreach (var panel in resourcePanels)
        {
            if (panel != null)
            {
                panel.OnActiveStateChanged -= HandlePanelActiveStateChanged;
            }
        }
    }
}