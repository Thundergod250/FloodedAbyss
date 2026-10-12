using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UIHUD : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI workerNumberText; // Worker count text

    [Header("Resource HUD Settings")]
    [SerializeField] private Transform resourceContainer;       // Parent layout group container
    [SerializeField] private GameObject resourcePairPrefab;     // Prefab with ResourcePairPanel attached

    private bool isSubscribed = false;
    private Dictionary<ResourceData, PanelResourcePair> resourcePanelMap = new();

    protected override void Initialize()
    {
        base.Initialize(); 
        if (resourcePairPrefab != null)
            resourcePairPrefab.SetActive(false); // Keep template hidden
    }

    protected override void OnEnable()
    {
        StartCoroutine(ConnectAndBuildHUDRoutine());
    }

    protected override void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator ConnectAndBuildHUDRoutine()
    {
        while (GameManager.Instance == null || GameManager.Instance.WorkerManager == null || GameManager.Instance.ResourceManager == null)
        {
            yield return null;
        }

        Unsubscribe();

        GameManager.Instance.WorkerManager.EvtOnWorkersChanged.AddListener(HandleWorkersChanged);
        GameManager.Instance.ResourceManager.OnResourceUpdated += HandleResourceUpdated;
        isSubscribed = true;

        HandleWorkersChanged(
            GameManager.Instance.WorkerManager.CurrentWorker, 
            GameManager.Instance.WorkerManager.MaximumWorker
        );

        BuildResourceHUD();
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
            }
            if (GameManager.Instance.ResourceManager != null)
            {
                GameManager.Instance.ResourceManager.OnResourceUpdated -= HandleResourceUpdated;
            }
            isSubscribed = false;
        }
    }

    private void HandleWorkersChanged(int current, int maximum)
    {
        if (workerNumberText != null) 
            workerNumberText.text = $"{current} / {maximum}";
    }

    private void HandleResourceUpdated(ResourceData data, int newAmount)
    {
        if (resourcePanelMap.TryGetValue(data, out PanelResourcePair panel))
        {
            if (panel != null) 
                panel.UpdateAmount(newAmount);
        }
    }
}