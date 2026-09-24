using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMiner : WorkableStructure
{
    [Header("Level Settings")]
    [SerializeField] private int minerLevel = 1;

    [Header("Resource Production (Level 1: Copper & Tin)")]
    [SerializeField]
    private List<ResourceType> targetResources = new List<ResourceType>
    {
        ResourceType.Copper,
        ResourceType.Tin
    };
    [SerializeField] private int baseYieldPerResource = 5;
    [SerializeField] private float harvestIntervalSeconds = 10f;

    [Header("Storage Settings")]
    [SerializeField] private int maxStoragePerResource = 100;
    private Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();

    private Coroutine autoMinerCoroutine;

    public int MinerLevel => minerLevel;
    public int MaxStoragePerResource => maxStoragePerResource;
    public Dictionary<ResourceType, int> StoredResources => storedResources;

    private void Awake()
    {
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

    private void InitializeStorage()
    {
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
        //int yieldAmount = Mathf.FloorToInt(baseYieldPerResource * TotalProductivity);

        /*if (yieldAmount <= 0) return;

        foreach (ResourceType resourceType in targetResources)
        {
            if (!storedResources.ContainsKey(resourceType))
                storedResources[resourceType] = 0;

            if (storedResources[resourceType] < maxStoragePerResource)
            {
                storedResources[resourceType] = Mathf.Min(storedResources[resourceType] + yieldAmount, maxStoragePerResource);
            }*/
        //}

        // Live update the AutoMiner UI panel if currently open
        UIAutoMiner uiMiner = FindAnyObjectByType<UIAutoMiner>(FindObjectsInactive.Include);
        /*if (uiMiner != null && uiMiner.gameObject.activeInHierarchy)
        {
            uiMiner.UpdateUI();
        }*/
    }

    /*
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
    */

    /*private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        //return FindAnyObjectByType<PlayerResources>();
    }*/
}