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

    private void Awake()
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
        {
            containerParent = transform;
        }

        // Enforce 5-panel maximum limit rule (dismiss oldest when reaching 4+)
        if (activePanels.Count >= 4 && activePanels.Count > 0)
        {
            UINotificationPanelPrefab oldestPanel = activePanels[0];
            activePanels.RemoveAt(0); // Unregister immediately
            oldestPanel.ForceDismiss();
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