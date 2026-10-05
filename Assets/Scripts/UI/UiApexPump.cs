using UnityEngine;
using System.Collections.Generic;

public class UiApexPump : UiModals
{
    [Header("Resource Panels")]
    [SerializeField] private List<PumpResourcePanel> resourcePanels = new List<PumpResourcePanel>();

    [Header("Water Settings when NO pumps are active")]
    [SerializeField] private float defaultMaxWaterHeight = 87f;

    [Header("Water Target Heights per Active Pump Count")]
    [Tooltip("Target height when 1 pump is active (Floor 1 level - 4 pumps lost)")]
    [SerializeField] private float targetHeight1Pump = 80f;

    [Tooltip("Target height when 2 pumps are active (Floor 2 level - 3 pumps lost)")]
    [SerializeField] private float targetHeight2Pumps = 70f;

    [Tooltip("Target height when 3 pumps are active (Floor 3 level - 2 pumps lost)")]
    [SerializeField] private float targetHeight3Pumps = 60f;

    [Tooltip("Target height when 4 pumps are active (Floor 4 level - 1 pump lost)")]
    [SerializeField] private float targetHeight4Pumps = 50f;

    [Tooltip("Target height when all 5 pumps are active (Floor 5 level - All Clear)")]
    [SerializeField] private float targetHeight5Pumps = 39f;

    [Header("Individual Pump Threshold Values")]
    [SerializeField] private float woodPumpThreshold = 50f;
    [SerializeField] private float stonePumpThreshold = 75f;
    [SerializeField] private float copperPumpThreshold = 100f;
    [SerializeField] private float ironPumpThreshold = 150f;
    [SerializeField] private float goldPumpThreshold = 200f;

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

        foreach (var panel in resourcePanels)
        {
            if (panel != null)
            {
                panel.Initialize(playerResources);

                panel.OnActiveStateChanged -= HandlePanelActiveStateChanged;
                panel.OnActiveStateChanged += HandlePanelActiveStateChanged;
            }
        }

        // Evaluate initial water state on game load
        RecalculateActivePumpsAndWaterLevel();
        LogAllPumpStates();
    }

    private void HandlePanelActiveStateChanged(PumpResourcePanel panel, bool isActive)
    {
        string state = isActive ? "ACTIVE" : "INACTIVE";
        Debug.Log($"[UiApexPump Notification] {panel.ResourceType} pump switched to {state}. Total Active Pumps: {GetActivePumpCount()}/{resourcePanels.Count}");

        RecalculateActivePumpsAndWaterLevel();
        LogAllPumpStates();
    }

    /// <summary>
    /// Recalculates water target height based purely on how many total pumps are active.
    /// Regardless of WHICH pump turns off, losing 1 pump moves target from 38m to 48m.
    /// </summary>
    private void RecalculateActivePumpsAndWaterLevel()
    {
        WaterLevel waterLevel = GameManager.Instance != null ? GameManager.Instance.waterLevel : null;

        if (waterLevel == null)
        {
            Debug.LogWarning("UiApexPump: GameManager.Instance.waterLevel reference is missing!");
            return;
        }

        int activeCount = 0;
        float totalThreshold = 0f;

        foreach (var panel in resourcePanels)
        {
            if (panel == null || !panel.IsActive) continue;

            activeCount++;

            switch (panel.ResourceType)
            {
                case ResourceType.Wood:
                    totalThreshold += woodPumpThreshold;
                    break;
                case ResourceType.Stone:
                    totalThreshold += stonePumpThreshold;
                    break;
                case ResourceType.Copper:
                    totalThreshold += copperPumpThreshold;
                    break;
                case ResourceType.Iron:
                    totalThreshold += ironPumpThreshold;
                    break;
                case ResourceType.Gold:
                    totalThreshold += goldPumpThreshold;
                    break;
            }
        }

        float targetHeight = GetTargetHeightForActiveCount(activeCount);

        if (activeCount > 0)
        {
            Debug.Log($"[UiApexPump] Active Pumps: {activeCount}/5. Water Level target set to: {targetHeight}m (Combined Threshold: {totalThreshold})");
            waterLevel.DrainToTargetHeight(targetHeight, totalThreshold);
        }
        else
        {
            Debug.Log($"[UiApexPump] 0 Pumps Active! Water rising back up to default height: {defaultMaxWaterHeight}m");
            waterLevel.OnAllPumpsDeactivated(defaultMaxWaterHeight);
        }
    }

    /// <summary>
    /// Maps the total count of running pumps to allowable water depth.
    /// </summary>
    private float GetTargetHeightForActiveCount(int activeCount)
    {
        switch (activeCount)
        {
            case 5: return targetHeight5Pumps; // 38m (All 5 active - lowest floor)
            case 4: return targetHeight4Pumps; // 48m (1 pump disabled - lose bottom floor)
            case 3: return targetHeight3Pumps; // 58m (2 pumps disabled)
            case 2: return targetHeight2Pumps; // 68m (3 pumps disabled)
            case 1: return targetHeight1Pump;  // 78m (4 pumps disabled - top floor only)
            default: return defaultMaxWaterHeight; // 87m (0 active pumps - max flood)
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