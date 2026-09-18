using System.Collections.Generic;
using UnityEngine;

public class UINotification : MonoBehaviour
{
    public static UINotification Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Transform containerParent;
    [SerializeField] private UINotificationPanelPrefab panelPrefab;

    [Header("Rules")]
    [SerializeField] private int maxVisiblePanels = 5;

    private List<UINotificationPanelPrefab> activePanels = new();

    private void Awake()
    {
        if (containerParent == null)
            containerParent = transform;
    }

    public void ShowNotification(string message, Color color)
    {
        if (panelPrefab == null || containerParent == null) return;

        // Rule: When spawning the 4th (or higher) item, begin fading out the 1st item immediately
        if (activePanels.Count >= 4 && activePanels.Count > 0)
        {
            UINotificationPanelPrefab oldestPanel = activePanels[0];
            activePanels.RemoveAt(0); // Unregister immediately so slot opens up
            oldestPanel.ForceDismiss();
        }

        // Spawn/Fetch from pool instead of standard Instantiate
        UINotificationPanelPrefab newPanel = Pool.Instantiate(panelPrefab, containerParent);
        newPanel.Setup(message, color, this);
        activePanels.Add(newPanel);
    }

    public void UnregisterPanel(UINotificationPanelPrefab panel)
    {
        if (activePanels.Contains(panel))
            activePanels.Remove(panel);
    }
}