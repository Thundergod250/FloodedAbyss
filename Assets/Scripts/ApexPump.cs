using UnityEngine;
using System;
using System.Collections.Generic;

public class ApexPump : MonoBehaviour
{
    [Header("Water Settings when NO pumps are active")]
    [SerializeField] private float defaultMaxWaterHeight = 87f;

    [Header("Water Target Heights per Active Pump Count")]
    [Tooltip("Target height when 1 pump is active")]
    [SerializeField] private float targetHeight1Pump = 80f;

    [Tooltip("Target height when 2 pumps are active")]
    [SerializeField] private float targetHeight2Pumps = 70f;

    [Tooltip("Target height when 3 pumps are active")]
    [SerializeField] private float targetHeight3Pumps = 60f;

    [Tooltip("Target height when 4 pumps are active")]
    [SerializeField] private float targetHeight4Pumps = 50f;

    [Tooltip("Target height when all 5 pumps are active")]
    [SerializeField] private float targetHeight5Pumps = 39f;

    [Header("Individual Pump Threshold Values")]
    [SerializeField] private float woodPumpThreshold = 50f;
    [SerializeField] private float stonePumpThreshold = 75f;
    [SerializeField] private float copperPumpThreshold = 100f;
    [SerializeField] private float ironPumpThreshold = 150f;
    [SerializeField] private float goldPumpThreshold = 200f;

    // Events for UI or other systems to subscribe to
    public event Action<PumpResourcePanel, bool> OnPanelActiveStateChanged;
    public event Action OnPumpStatesRecalculated;

    private PlayerResources playerResources;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }

        if (playerResources == null)
        {
            Debug.LogError("ApexPump: PlayerResources could not be found!");
        }
    }

    /// <summary>
    /// Called by the UI layer when panels are initialized and ready to report state changes.
    /// </summary>
    public void RegisterPanelListeners(List<PumpResourcePanel> panels)
    {
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                if (playerResources != null)
                {
                    panel.Initialize(playerResources);
                }

                panel.OnActiveStateChanged -= HandlePanelActiveStateChanged;
                panel.OnActiveStateChanged += HandlePanelActiveStateChanged;
            }
        }

        // Evaluate initial water state once panels are hooked up
        RecalculateWaterLevelBasedOnPanels(panels);
    }

    private void HandlePanelActiveStateChanged(PumpResourcePanel panel, bool isActive)
    {
        // Forward event to UI listeners
        OnPanelActiveStateChanged?.Invoke(panel, isActive);
    }

    public void RecalculateWaterLevelBasedOnPanels(List<PumpResourcePanel> panels)
    {
        WaterLevel waterLevel = GameManager.Instance != null ? GameManager.Instance.waterLevel : null;

        if (waterLevel == null)
        {
            Debug.LogWarning("ApexPump: GameManager.Instance.waterLevel reference is missing!");
            return;
        }

        int activeCount = 0;
        float totalThreshold = 0f;

        foreach (var panel in panels)
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
            Debug.Log($"[ApexPump] Active Pumps: {activeCount}/5. Water Level target set to: {targetHeight}m (Combined Threshold: {totalThreshold})");
            waterLevel.DrainToTargetHeight(targetHeight, totalThreshold);
        }
        else
        {
            Debug.Log($"[ApexPump] 0 Pumps Active! Water rising back up to default height: {defaultMaxWaterHeight}m");
            waterLevel.OnAllPumpsDeactivated(defaultMaxWaterHeight);
        }

        OnPumpStatesRecalculated?.Invoke();
    }

    private float GetTargetHeightForActiveCount(int activeCount)
    {
        switch (activeCount)
        {
            case 5: return targetHeight5Pumps;
            case 4: return targetHeight4Pumps;
            case 3: return targetHeight3Pumps;
            case 2: return targetHeight2Pumps;
            case 1: return targetHeight1Pump;
            default: return defaultMaxWaterHeight;
        }
    }
}