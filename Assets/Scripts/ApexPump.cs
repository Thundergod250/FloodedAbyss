using UnityEngine;
using System;
using System.Collections.Generic;

public class ApexPump : MonoBehaviour
{
    [Header("Water Settings when NO pumps are active")]
    [SerializeField] private float defaultMaxWaterHeight = 87f;

    [Header("Water Target Heights per Active Pump Count")]
    [SerializeField] private float targetHeight1Pump = 80f;
    [SerializeField] private float targetHeight2Pumps = 70f;
    [SerializeField] private float targetHeight3Pumps = 60f;
    [SerializeField] private float targetHeight4Pumps = 50f;
    [SerializeField] private float targetHeight5Pumps = 39f;

    [Header("Individual Base Pump Threshold Values")]
    [SerializeField] private float woodPumpThreshold = 50f;
    [SerializeField] private float stonePumpThreshold = 75f;
    [SerializeField] private float copperPumpThreshold = 100f;
    [SerializeField] private float ironPumpThreshold = 150f;
    [SerializeField] private float goldPumpThreshold = 200f;

    private Dictionary<ResourceType, int> pumpThresholdUpgradeLevels = new Dictionary<ResourceType, int>();
    [Header("Threshold Upgrade Scaling")]
    [SerializeField] private float thresholdIncreasePerLevel = 25f;

    public event Action<PumpResourcePanel, bool> OnPanelActiveStateChanged;
    public event Action OnPumpStatesRecalculated;

    private PlayerResources playerResources;
    private List<PumpResourcePanel> registeredPanels = new List<PumpResourcePanel>();

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

    public void RegisterPanelListeners(List<PumpResourcePanel> panels)
    {
        registeredPanels = panels;

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

        RecalculateWaterLevelBasedOnPanels(registeredPanels);
    }

    private void HandlePanelActiveStateChanged(PumpResourcePanel panel, bool isActive)
    {
        OnPanelActiveStateChanged?.Invoke(panel, isActive);
    }

    public void SyncThresholdLevelFromPlayerStats(ResourceType type, int newLevel)
    {
        pumpThresholdUpgradeLevels[type] = newLevel;
        Debug.Log($"[ApexPump] Synced {type} Pump threshold level from PlayerStats to: {newLevel}");
        RecalculateWaterLevelBasedOnPanels(registeredPanels);
    }

    public float GetThresholdForPump(ResourceType type)
    {
        float baseThreshold = type switch
        {
            ResourceType.Wood => woodPumpThreshold,
            ResourceType.Stone => stonePumpThreshold,
            ResourceType.Copper => copperPumpThreshold,
            ResourceType.Iron => ironPumpThreshold,
            ResourceType.Gold => goldPumpThreshold,
            _ => 50f
        };

        int level = pumpThresholdUpgradeLevels.TryGetValue(type, out var lvl) ? lvl : 0;
        return baseThreshold + (level * thresholdIncreasePerLevel);
    }

    public int GetThresholdUpgradeLevel(ResourceType type)
    {
        return pumpThresholdUpgradeLevels.TryGetValue(type, out var lvl) ? lvl : 0;
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

        if (panels != null)
        {
            foreach (var panel in panels)
            {
                if (panel == null || !panel.IsActive) continue;

                activeCount++;
                totalThreshold += GetThresholdForPump(panel.ResourceType);
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
        return activeCount switch
        {
            5 => targetHeight5Pumps,
            4 => targetHeight4Pumps,
            3 => targetHeight3Pumps,
            2 => targetHeight2Pumps,
            1 => targetHeight1Pump,
            _ => defaultMaxWaterHeight
        };
    }
}