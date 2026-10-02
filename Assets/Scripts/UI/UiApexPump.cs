using UnityEngine;
using System.Collections.Generic;

public class UiApexPump : UiModals
{
    [Header("Resource Panels")]
    [SerializeField] private List<PumpResourcePanel> resourcePanels = new List<PumpResourcePanel>();

    [Header("Water Settings when NO pumps are active")]
    [SerializeField] private float defaultMaxWaterHeight = 87f;

    [Header("Floor Level Heights")]
    [SerializeField] private float floor1WoodHeight = 78f;
    [SerializeField] private float floor2StoneHeight = 68f;
    [SerializeField] private float floor3CopperHeight = 58f;
    [SerializeField] private float floor4IronHeight = 48f;
    [SerializeField] private float floor5GoldHeight = 38f;

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
    /// Recalculates allowable water target depth based on tier progression.
    /// To reach a lower floor, ALL preceding pumps must remain active.
    /// </summary>
    private void RecalculateActivePumpsAndWaterLevel()
    {
        WaterLevel waterLevel = GameManager.Instance != null ? GameManager.Instance.waterLevel : null;

        if (waterLevel == null)
        {
            Debug.LogWarning("UiApexPump: GameManager.Instance.waterLevel reference is missing!");
            return;
        }

        // Track active status for each specific pump tier
        bool isWoodActive = IsPumpActive(ResourceType.Wood);
        bool isStoneActive = IsPumpActive(ResourceType.Stone);
        bool isCopperActive = IsPumpActive(ResourceType.Copper);
        bool isIronActive = IsPumpActive(ResourceType.Iron);
        bool isGoldActive = IsPumpActive(ResourceType.Gold);

        // Determine target water height based on tier chain:
        // If a tier is missing, water cannot drain lower than that tier's floor!
        float targetHeight = defaultMaxWaterHeight;

        if (isWoodActive)
        {
            targetHeight = floor1WoodHeight; // 78m

            if (isStoneActive)
            {
                targetHeight = floor2StoneHeight; // 68m

                if (isCopperActive)
                {
                    targetHeight = floor3CopperHeight; // 58m

                    if (isIronActive)
                    {
                        targetHeight = floor4IronHeight; // 48m

                        if (isGoldActive)
                        {
                            targetHeight = floor5GoldHeight; // 38m (All 5 Active)
                        }
                    }
                }
            }
        }
        else if (isGoldActive || isIronActive || isCopperActive || isStoneActive)
        {
            // If Wood is OFF, but lower pumps are ON, water rises up to Floor 1 (78m) 
            // because you lost the top-tier drainage foundation.
            targetHeight = floor1WoodHeight;
        }

        // Calculate total combined threshold of ALL currently running pumps
        float totalThreshold = 0f;
        if (isWoodActive) totalThreshold += woodPumpThreshold;
        if (isStoneActive) totalThreshold += stonePumpThreshold;
        if (isCopperActive) totalThreshold += copperPumpThreshold;
        if (isIronActive) totalThreshold += ironPumpThreshold;
        if (isGoldActive) totalThreshold += goldPumpThreshold;

        int activeCount = GetActivePumpCount();

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

    private bool IsPumpActive(ResourceType type)
    {
        foreach (var panel in resourcePanels)
        {
            if (panel != null && panel.ResourceType == type)
            {
                return panel.IsActive;
            }
        }
        return false;
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