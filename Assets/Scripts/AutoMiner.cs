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

    [Header("Tiered Harvest Speeds (Seconds per Harvest Cycle)")]
    [SerializeField] private float stoneHarvestInterval = 4f;
    [SerializeField] private float copperHarvestInterval = 8f;
    [SerializeField] private float ironHarvestInterval = 16f;
    [SerializeField] private float goldHarvestInterval = 32f;

    [Header("Storage Settings")]
    [SerializeField] private int maxStoragePerResource = 100;
    private Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, float> resourceProgressTimers = new Dictionary<ResourceType, float>();

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

    protected override void Start()
    {
        waterLevel = GameManager.Instance.waterLevel;
    }

    private void Update()
    {
        if (IsBroken || IsSubmerged) return;

        float productivity = TotalProductivity;
        if (productivity <= 0f) return;

        foreach (ResourceType resourceType in targetResources)
        {
            if (!resourceProgressTimers.ContainsKey(resourceType))
                resourceProgressTimers[resourceType] = 0f;

            float interval = GetHarvestInterval(resourceType);
            resourceProgressTimers[resourceType] += Time.deltaTime * productivity;

            if (resourceProgressTimers[resourceType] >= interval)
            {
                resourceProgressTimers[resourceType] = 0f;
                HarvestSingleResource(resourceType);
            }
        }
    }

    private float GetHarvestInterval(ResourceType type)
    {
        return type switch
        {
            ResourceType.Stone => stoneHarvestInterval,
            ResourceType.Copper => copperHarvestInterval,
            ResourceType.Iron => ironHarvestInterval,
            ResourceType.Gold => goldHarvestInterval,
            _ => 10f
        };
    }

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
            targetResources.Add(ResourceType.Copper);
        }
        else if (level == 2)
        {
            maxWorkers = 4;
            targetResources.Add(ResourceType.Stone);
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

            if (!resourceProgressTimers.ContainsKey(resourceType))
                resourceProgressTimers[resourceType] = 0f;
        }
    }

    public override void Activate()
    {
        if (IsSubmerged || IsBroken)
        {
            if (IsBroken) ShowRepairPrompt();
            return;
        }

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

    private void HarvestSingleResource(ResourceType resourceType)
    {
        int yieldAmount = Mathf.FloorToInt(baseYieldPerResource * TotalProductivity);
        if (yieldAmount <= 0) return;

        if (!storedResources.ContainsKey(resourceType))
            storedResources[resourceType] = 0;

        if (storedResources[resourceType] < maxStoragePerResource)
        {
            storedResources[resourceType] = Mathf.Min(storedResources[resourceType] + yieldAmount, maxStoragePerResource);
        }

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