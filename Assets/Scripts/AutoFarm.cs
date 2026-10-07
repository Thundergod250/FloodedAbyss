using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFarm : WorkableStructure
{
    [Header("Level Settings")]
    [Tooltip("Manually switch between Level 1 and Level 2 in the Inspector.")]
    [Range(1, 2)]
    [SerializeField] private int farmLevel = 1;

    [Header("Resource Production")]
    [SerializeField] private List<ResourceType> targetResources = new List<ResourceType>();
    [SerializeField] private int baseYieldPerResource = 5;
    [SerializeField] private float harvestIntervalSeconds = 10f;

    [Header("Storage Settings")]
    [SerializeField] private int maxStoragePerResource = 100;
    private Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();

    private Coroutine autoFarmCoroutine;

    public int FarmLevel => farmLevel;
    public int MaxStoragePerResource => maxStoragePerResource;
    public Dictionary<ResourceType, int> StoredResources => storedResources;

    private void OnValidate()
    {
        ApplyLevelRules(farmLevel);
    }

    private void Awake()
    {
        ApplyLevelRules(farmLevel);
        InitializeStorage();
    }

    protected override void Start()
    {

    }

    private void OnEnable()
    {
        if (autoFarmCoroutine != null)
        {
            StopCoroutine(autoFarmCoroutine);
        }
        autoFarmCoroutine = StartCoroutine(AutoFarmProductionRoutine());
    }

    private void OnDisable()
    {
        if (autoFarmCoroutine != null)
        {
            StopCoroutine(autoFarmCoroutine);
        }
    }

    public void SetLevel(int level)
    {
        farmLevel = Mathf.Clamp(level, 1, 2);
        ApplyLevelRules(farmLevel);
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
            targetResources.Add(ResourceType.Wood);
        }
        else if (level == 2)
        {
            maxWorkers = 4;
            targetResources.Add(ResourceType.Wood);
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
        UIAutoFarm uiFarm = FindAnyObjectByType<UIAutoFarm>(FindObjectsInactive.Include);
        if (uiFarm != null)
        {
            uiFarm.OpenAutoFarmMenu(this);
        }
        else if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.AutoFarm);
        }
    }

    private IEnumerator AutoFarmProductionRoutine()
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

        UIAutoFarm uiFarm = FindAnyObjectByType<UIAutoFarm>(FindObjectsInactive.Include);
        if (uiFarm != null && uiFarm.gameObject.activeInHierarchy)
        {
            uiFarm.UpdateUI();
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