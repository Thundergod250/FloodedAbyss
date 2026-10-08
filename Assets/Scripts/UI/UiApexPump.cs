using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UiApexPump : UiModals
{
    [Header("Resource Panels UI")]
    [SerializeField] private List<PumpResourcePanel> resourcePanels = new List<PumpResourcePanel>();

    public List<PumpResourcePanel> ResourcePanels => resourcePanels;

    protected override void Start()
    {
        base.Start();

        if (GameManager.Instance != null && GameManager.Instance.ApexPump != null)
        {
            GameManager.Instance.ApexPump.OnPanelActiveStateChanged += HandlePanelActiveStateChanged;
            GameManager.Instance.ApexPump.OnPumpStatesRecalculated += HandlePumpStatesRecalculated;

            // Hand over the panels to the core logic script to set up data listeners & initial water level
            StartCoroutine(RegisterDelay());
        }
        else
        {
            Debug.LogError("UiApexPump: GameManager or ApexPump reference is missing!");
        }

        LogAllPumpStates();
    }

    private void HandlePanelActiveStateChanged(PumpResourcePanel panel, bool isActive)
    {
        string state = isActive ? "ACTIVE" : "INACTIVE";
        Debug.Log($"[UiApexPump Notification] {panel.ResourceType} pump switched to {state}. Total Active Pumps: {GetActivePumpCount()}/{resourcePanels.Count}");

        // Tell core logic to update the water level based on the UI panels' current states
        if (GameManager.Instance != null && GameManager.Instance.ApexPump != null)
        {
            GameManager.Instance.ApexPump.RecalculateWaterLevelBasedOnPanels(resourcePanels);
        }

        LogAllPumpStates();
    }

    private void HandlePumpStatesRecalculated()
    {
        // Add any UI refresh logic here if your panels need to redraw based on updated states
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
        if (GameManager.Instance != null && GameManager.Instance.ApexPump != null)
        {
            GameManager.Instance.ApexPump.OnPanelActiveStateChanged -= HandlePanelActiveStateChanged;
            GameManager.Instance.ApexPump.OnPumpStatesRecalculated -= HandlePumpStatesRecalculated;
        }
    }

    private IEnumerator RegisterDelay()
    {
        yield return new WaitForSeconds(0.25f);

        GameManager.Instance.ApexPump.RegisterPanelListeners(resourcePanels);
    }
}