using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIHUD : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI workerNumberText;     // Worker count text
    [SerializeField] private TextMeshProUGUI homelessWorkerText;  // Homeless worker count text

    [Header("Water HUD References")]
    [SerializeField] private TextMeshProUGUI waterLevelText;       // Middle Big UI (e.g., "95 m")
    [SerializeField] private TextMeshProUGUI dischargePressureText; // Bottom Left Green Text (e.g., "1 m/s")
    [SerializeField] private TextMeshProUGUI waterPressureText;     // Bottom Right Red Text (e.g., "1 m/s")

    [Header("Resource HUD Settings")]
    [SerializeField] private Transform resourceContainer;       
    [SerializeField] private GameObject resourcePairPrefab;     

    private bool isSubscribed = false;
    private Dictionary<ResourceDataSO, PanelResourcePair> resourcePanelMap = new();

    protected override void Initialize()
    {
        base.Initialize(); 
        if (resourcePairPrefab != null)
            resourcePairPrefab.SetActive(false); 
    }

    protected override void OnEnable()
    {
        StartCoroutine(ConnectAndBuildHUDRoutine());
    }

    protected override void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        // Continuously update water statistics on the persistent HUD
        UpdateWaterHUD();
    }

    private IEnumerator ConnectAndBuildHUDRoutine()
    {
        while (GameManager.Instance == null || GameManager.Instance.WorkerManager == null || GameManager.Instance.ResourceManager == null)
        {
            yield return null;
        }

        Unsubscribe();

        GameManager.Instance.WorkerManager.EvtOnWorkersChanged.AddListener(HandleWorkersChanged);
        GameManager.Instance.WorkerManager.EvtOnHomelessChanged.AddListener(HandleHomelessChanged);
        GameManager.Instance.ResourceManager.OnResourceUpdated += HandleResourceUpdated;
        isSubscribed = true;

        HandleWorkersChanged(GameManager.Instance.WorkerManager.CurrentWorker);
        HandleHomelessChanged(GameManager.Instance.WorkerManager.HomelessWorker);

        BuildResourceHUD();
    }

    private void UpdateWaterHUD()
    {
        if (GameManager.Instance == null || GameManager.Instance.WaterLevel == null) return;

        WaterLevel waterMgr = GameManager.Instance.WaterLevel;

        // Update Water Level (Middle Big Text)
        if (waterLevelText != null)
        {
            waterLevelText.text = $"{Mathf.RoundToInt(waterMgr.CurrentWaterHeight)} m";
        }

        // Update Discharge Pressure / Pump strength (Bottom Left Green Text)
        if (dischargePressureText != null)
        {
            dischargePressureText.text = $"{waterMgr.NetDischargePower:F0} m/s";
        }

        // Update Water Pressure (Bottom Right Red Text - now handles integer pressure rates cleanly)
        if (waterPressureText != null)
        {
            waterPressureText.text = $"{waterMgr.WaterPressure} m/s";
        }
    }

    private void BuildResourceHUD()
    {
        if (resourceContainer == null || resourcePairPrefab == null) return;

        foreach (Transform child in resourceContainer)
        {
            if (child.gameObject != resourcePairPrefab)
            {
                Destroy(child.gameObject);
            }
        }

        resourcePanelMap.Clear();
        var resourceManager = GameManager.Instance.ResourceManager;

        foreach (var resourceData in resourceManager.AvailableResources)
        {
            if (resourceData == null) continue;

            GameObject itemGO = Instantiate(resourcePairPrefab, resourceContainer);
            itemGO.SetActive(true);

            PanelResourcePair panel = itemGO.GetComponent<PanelResourcePair>();
            if (panel != null)
            {
                int currentAmount = resourceManager.GetResourceAmount(resourceData);
                panel.Setup(resourceData, currentAmount);
                resourcePanelMap[resourceData] = panel;
            }
        }
    }

    private void Unsubscribe()
    {
        if (isSubscribed && GameManager.Instance != null)
        {
            if (GameManager.Instance.WorkerManager != null)
            {
                GameManager.Instance.WorkerManager.EvtOnWorkersChanged.RemoveListener(HandleWorkersChanged);
                GameManager.Instance.WorkerManager.EvtOnHomelessChanged.RemoveListener(HandleHomelessChanged);
            }
            if (GameManager.Instance.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.OnResourceUpdated -= HandleResourceUpdated;
            }
            isSubscribed = false;
        }
    }

    private void HandleWorkersChanged(int current)
    {
        if (workerNumberText != null) 
            workerNumberText.text = $"Workers {current}";
    }

    private void HandleHomelessChanged(int homelessCount)
    {
        if (homelessWorkerText != null)
        {
            homelessWorkerText.text = homelessCount > 0 ? $"Homeless Workers {homelessCount}" : string.Empty;
        }
    }

    private void HandleResourceUpdated(ResourceDataSO dataSo, int newAmount)
    {
        if (resourcePanelMap.TryGetValue(dataSo, out PanelResourcePair panel))
        {
            if (panel != null) 
                panel.UpdateAmount(newAmount);
        }
    }
}