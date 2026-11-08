using System.Resources;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class WorkerManager : MonoBehaviour
{
    [field: SerializeField] public int CurrentWorker { get; private set; } = 1;
    [field: SerializeField] public int HomelessWorker { get; private set; } = 1; // Starts with 1 homeless worker
    [field: SerializeField] public int OccupiedHousesCount { get; private set; } = 0;
    [field: SerializeField] public int TotalHousingCapacity { get; private set; }

    [Header("Events")]
    public UnityEvent<int> EvtOnWorkersChanged = new();
    public UnityEvent<int> EvtOnHomelessChanged = new();
    public UnityEvent<int> EvtOnWorkerReduced = new();

    [Header("Worker Resource Consumption")]
    [SerializeField] private ResourceDataSO resourceToConsume;
    [SerializeField] private int consumptionPerWorker = 1;
    [SerializeField] private float consumptionInterval = 10f;

    private List<StructureWorkersHouse> workerHouses = new List<StructureWorkersHouse>();

    private void Start() 
    {
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
        EvtOnHomelessChanged?.Invoke(HomelessWorker);

       StartCoroutine(WorkerConsumptionRoutine());
    }

    public void SetCurrentWorkers(int amount)
    {
        CurrentWorker = Mathf.Max(0, amount);
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
    }

    public void AddHomelessWorker(int amount)
    {
        HomelessWorker = Mathf.Max(0, HomelessWorker + amount);
        EvtOnHomelessChanged?.Invoke(HomelessWorker);
    }

    public void ReduceWorker(int val)
    {
        if (val <= 0 || CurrentWorker <= 0)
            return;

        int workersToRemove = Mathf.Min(val, CurrentWorker);

        for (int i = 0; i < workersToRemove; i++)
        {
            StructureWorkersHouse occupiedHouse =
                workerHouses.Find(h => h != null && h.IsOccupied);

            if (occupiedHouse != null)
            {
                occupiedHouse.RemoveWorkerDueToDeath();
                OccupiedHousesCount--;
            }
        }

        CurrentWorker -= workersToRemove;

        EvtOnWorkersChanged?.Invoke(CurrentWorker);
        EvtOnWorkerReduced?.Invoke(CurrentWorker);
    }

    public bool TryHouseWorker()
    {
        if (HomelessWorker > 0)
        {
            HomelessWorker--;
            OccupiedHousesCount++;
            EvtOnHomelessChanged?.Invoke(HomelessWorker);
            return true;
        }
        return false;
    }

    public void FreeHouse()
    {
        if (OccupiedHousesCount > 0)
        {
            OccupiedHousesCount--;
        }
        // When an occupied house is destroyed, the housed worker becomes homeless again!
        HomelessWorker++;
        EvtOnHomelessChanged?.Invoke(HomelessWorker);
    }

    public void RecruitNewWorker(Structures recruitingBuilding)
    {
        if (recruitingBuilding == null)
            return;

        CurrentWorker++;
        EvtOnWorkersChanged?.Invoke(CurrentWorker);

        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.NotifyWorkerRecruited(recruitingBuilding);
        }
    }

    public void CancelHouseReservation()
    {
        if (OccupiedHousesCount > 0)
        {
            OccupiedHousesCount--;
        }
        HomelessWorker++;
        EvtOnHomelessChanged?.Invoke(HomelessWorker);
    }

    private IEnumerator WorkerConsumptionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(consumptionInterval);

            if (resourceToConsume == null)
                continue;

            int totalConsumption = CurrentWorker * consumptionPerWorker;

            if (totalConsumption > 0)
            {
                GameManager.Instance.ResourceManager.ConsumeResource(
                    resourceToConsume,
                    totalConsumption
                );
            }
        }
    }

    public void AddHousingCapacity(int amount)
    {
        TotalHousingCapacity += amount;
    }

    public void RegisterWorkerHouse(StructureWorkersHouse house)
    {
        if (house != null && !workerHouses.Contains(house))
            workerHouses.Add(house);
    }

    public void UnregisterWorkerHouse(StructureWorkersHouse house)
    {
        if (house != null)
            workerHouses.Remove(house);
    }
}