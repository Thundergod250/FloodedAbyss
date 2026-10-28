using System.Collections.Generic;
using UnityEngine;

public class UINotification : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform containerParent;
    [SerializeField] private UINotificationPanelPrefab panelPrefab;

    [Header("Internal Spawner Test")]
    [SerializeField] private bool enableSelfTesting = false;
    [SerializeField] private float testSpawnInterval = 0.8f;

    private List<UINotificationPanelPrefab> activePanels = new();
    private float testTimer;
    private int testCounter = 1;

    private readonly Color[] testColors = new Color[] 
    { 
        Color.green, 
        Color.red, 
        Color.yellow, 
        Color.cyan 
    };

    private void Start()
    {
        if (containerParent == null)
            containerParent = transform;
    }

    private void Update()
    {
        // Self-testing auto spawner built directly into UINotification
        if (!enableSelfTesting) return;

        testTimer += Time.deltaTime;
        if (testTimer >= testSpawnInterval)
        {
            testTimer = 0f;
            Color randomColor = testColors[Random.Range(0, testColors.Length)];
            ShowNotification($"+{testCounter * 10} Test Resource #{testCounter}", randomColor);
            testCounter++;
        }
    }

    public void ShowNotification(string message, Color color)
    {
        if (panelPrefab == null)
        {
            Debug.LogError("[UINotification] Panel Prefab reference is missing in Inspector!");
            return;
        }

        if (containerParent == null) 
            containerParent = transform;

        // Clean up any destroyed or null panels before enforcement
        activePanels.RemoveAll(panel => panel == null);

        // Enforce maximum active panel limit
        while (activePanels.Count >= 4)
        {
            UINotificationPanelPrefab oldestPanel = activePanels[0];
            // ForceDismiss() will invoke UnregisterPanel() internally to cleanly remove itself
            oldestPanel.ForceDismiss();
            
            // Backup check in case ForceDismiss was called on an already inactive object
            if (activePanels.Count > 0 && activePanels[0] == oldestPanel)
            {
                activePanels.RemoveAt(0);
            }
        }

        // Fetch / Spawn from pool
        UINotificationPanelPrefab newPanel = Pool.Instantiate(panelPrefab, containerParent);

        if (newPanel != null)
        {
            newPanel.Setup(message, color, this);
            activePanels.Add(newPanel);
        }
    }

    public void UnregisterPanel(UINotificationPanelPrefab panel)
    {
        if (activePanels.Contains(panel))
            activePanels.Remove(panel);
    }
}