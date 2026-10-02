using UnityEngine;
using System.Collections.Generic;

public class UiApexPump : UiModals
{
    [Header("Resource Panels")]
    [SerializeField] private List<PumpResourcePanel> resourcePanels = new List<PumpResourcePanel>();

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

        // Initialize every individual resource panel
        foreach (var panel in resourcePanels)
        {
            if (panel != null)
            {
                panel.Initialize(playerResources);
            }
        }
    }

    /// <summary>
    /// Helper method to calculate combined total energy generated across all 5 panels.
    /// </summary>
    public int GetTotalEnergy()
    {
        int total = 0;
        foreach (var panel in resourcePanels)
        {
            if (panel != null) total += panel.CurrentEnergy;
        }
        return total;
    }
}