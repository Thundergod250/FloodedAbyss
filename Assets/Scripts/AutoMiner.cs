using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMiner : WorkableStructure
{
    [Header("Level Settings")]
    [Tooltip("Manually switch between Level 1 and Level 2 in the Inspector.")]
    [Range(1, 2)]
    [SerializeField] private int minerLevel = 1;

    [Header("Resource Production")]
    [SerializeField] private List<ResourceType> targetResources = new List<ResourceType>();
    [SerializeField] private int baseYieldPerResource = 5;
    [SerializeField] private float harvestIntervalSeconds = 10f;

    [Header("Storage Settings")]
    [SerializeField] private int maxStoragePerResource = 100;
    private Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();

    private Coroutine autoMinerCoroutine;

    public int MinerLevel => minerLevel;
    public int MaxStoragePerResource => maxStoragePerResource;
    public Dictionary<ResourceType, int> StoredResources => storedResources;

    private void OnValidate()
    {
        ApplyLevelRules(minerLevel);
    }

    private void Awake()
    {
        ApplyLevelRules(minerLevel);
        InitializeStorage();
    }

    /// <summary>
    /// Overriding Start prevents WorkableStructure's base ProductionCycleRoutine 
    /// from running and giving Gold directly to the player inventory.
    /// </summary>
    protected override void Start()
    {
        // Intentionally left blank to override base.Start()
    }

    private void OnEnable()
    {
        if (autoMinerCoroutine != null)
        {
            StopCoroutine(autoMinerCoroutine);
        }
        autoMinerCoroutine = StartCoroutine(AutoMinerProductionRoutine());
    }

    private void OnDisable()
    {
        if (autoMinerCoroutine != null)
        {
            StopCoroutine(autoMinerCoroutine);
        }
    }

    /// <summary>
    /// Manually or programmatically set the miner level (1 or 2).
    /// </summary>
    public void SetLevel(int level)
    {
        minerLevel = Mathf.Clamp(level, 1, 2);
        ApplyLevelRules(minerLevel);
    }

    [ContextMenu("Switch to Level 1")]
    public void SetToLevel1() => SetLevel(1);

    [ContextMenu("Switch to Level 2")]
    public void SetToLevel2() => SetLevel(2);

    private void ApplyLevelRules(int level)
    {
        targetResources.Clear();

        if (level == 1)
        {
            maxWorkers = 2;
            targetResources.Add(ResourceType.Stone);
            targetResources.Add(ResourceType.Tin);
            targetResources.Add(ResourceType.Copper);
        }
        else if (level == 2)
        {
            maxWorkers = 4;
            targetResources.Add(ResourceType.Stone);
            targetResources.Add(ResourceType.Tin);
            targetResources.Add(ResourceType.Copper);
            targetResources.Add(ResourceType.Iron);
            targetResources.Add(ResourceType.Gold);
        }

        InitializeStorage();
    }

    private void InitializeStorage()
    {
        if (storedResources == null)
            storedResources = new Dictionary<ResourceType, int>();

        foreach (ResourceType resourceType in targetResources)
        {
            if (!storedResources.ContainsKey(resourceType))
                storedResources[resourceType] = 0;
        }
    }

    public override void Activate()
    {
        UIAutoMiner uiMiner = FindAnyObjectByType<UIAutoMiner>(FindObjectsInactive.Include);
        if (uiMiner != null)
        {
            uiMiner.OpenAutoMinerMenu(this);
        }
        else if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.AutoMiner);
        }
    }

    private IEnumerator AutoMinerProductionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(harvestIntervalSeconds);
            HarvestToStorage();
        }
    }

    private void HarvestToStorage()
    {
        int yieldAmount = Mathf.FloorToInt(baseYieldPerResource * TotalProductivity);

        if (yieldAmount <= 0) return;

        foreach (ResourceType resourceType in targetResources)
        {
            if (!storedResources.ContainsKey(resourceType))
                storedResources[resourceType] = 0;

            if (storedResources[resourceType] < maxStoragePerResource)
            {
                storedResources[resourceType] = Mathf.Min(storedResources[resourceType] + yieldAmount, maxStoragePerResource);
            }
        }

        // Live update the AutoMiner UI panel if currently open
        UIAutoMiner uiMiner = FindAnyObjectByType<UIAutoMiner>(FindObjectsInactive.Include);
        if (uiMiner != null && uiMiner.gameObject.activeInHierarchy)
        {
            uiMiner.UpdateUI();
        }
    }

    public void ClaimResources()
    {
        PlayerResources playerResources = GetPlayerResources();
        if (playerResources == null) return;

        List<ResourceType> keys = new List<ResourceType>(storedResources.Keys);
        foreach (ResourceType resource in keys)
        {
            int amountToClaim = storedResources[resource];
            if (amountToClaim > 0)
            {
                playerResources.AddResource(resource, amountToClaim);
                storedResources[resource] = 0;
                Debug.Log($"[{buildingName}] Claimed {amountToClaim} {resource}. Storage reset to 0.");
            }
        }
    }

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }
}