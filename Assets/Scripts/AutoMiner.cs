using System.Collections;
using UnityEngine;

public class AutoMiner : MonoBehaviour
{
    [Header("Resource Production")]
    [SerializeField] private ResourceType resourceType = ResourceType.Stone;
    [SerializeField] private int baseYieldAmount = 10;
    [SerializeField] private float harvestIntervalSeconds = 10f;

    [Header("Population & Productivity")]
    [SerializeField] private int maxWorkerCapacity = 5;
    [SerializeField] private int assignedWorkers = 5;

    private Coroutine miningCoroutine;

    private void OnEnable()
    {
        miningCoroutine = StartCoroutine(MiningRoutine());
    }

    private void OnDisable()
    {
        if (miningCoroutine != null)
        {
            StopCoroutine(miningCoroutine);
        }
    }

    private IEnumerator MiningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(harvestIntervalSeconds);
            Harvest();
        }
    }

    public void Harvest()
    {
        PlayerResources playerResources = GetPlayerResources();
        if (playerResources == null) return;

        float efficiency = CalculateProductivityMultiplier();
        int finalYield = Mathf.RoundToInt(baseYieldAmount * efficiency);

        if (finalYield > 0)
        {
            playerResources.AddResource(resourceType, finalYield);

            Debug.Log($"[Auto-Miner] Generated +{finalYield} {resourceType}!");
        }
    }

    public float CalculateProductivityMultiplier()
    {
        float staffingRate = maxWorkerCapacity > 0 ? (float)assignedWorkers / maxWorkerCapacity : 1f;

        float globalHappinessMultiplier = GetGlobalHappinessMultiplier();

        return staffingRate * globalHappinessMultiplier;
    }

    private float GetGlobalHappinessMultiplier()
    {
        return 1.0f;
    }

    public void SetAssignedWorkers(int count)
    {
        assignedWorkers = Mathf.Clamp(count, 0, maxWorkerCapacity);
    }

    public int AssignedWorkers => assignedWorkers;
    public int MaxWorkerCapacity => maxWorkerCapacity;

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }
}