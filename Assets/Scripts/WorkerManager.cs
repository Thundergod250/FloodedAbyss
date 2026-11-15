using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WorkerManager : MonoBehaviour
{
    [field: SerializeField] public int CurrentWorker { get; private set; } = 0;
    [field: SerializeField] public int TotalHousingCapacity { get; private set; } = 0;

    [Header("Events")]
    public UnityEvent<int> EvtOnWorkersChanged = new();
    public UnityEvent<int> EvtOnWorkerReduced = new();

    private List<StructureWorkersHouse> workerHouses = new();

    private void Start() => EvtOnWorkersChanged?.Invoke(CurrentWorker);

    public void SetCurrentWorkers(int amount)
    {
        CurrentWorker = Mathf.Max(0, amount);
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
    }

    // Called when a new house is built
    public void RegisterHouse(int capacity, int initialWorkers)
    {
        TotalHousingCapacity += capacity;
        CurrentWorker += initialWorkers;
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
    }

    // Called when a house is destroyed (Workers in the house die)
    public void UnregisterHouse(int capacity, int housedWorkers)
    {
        TotalHousingCapacity = Mathf.Max(0, TotalHousingCapacity - capacity);
        
        int workersLost = Mathf.Min(CurrentWorker, housedWorkers);
        CurrentWorker = Mathf.Max(0, CurrentWorker - workersLost);

        EvtOnWorkersChanged?.Invoke(CurrentWorker);
        if (workersLost > 0) 
            EvtOnWorkerReduced?.Invoke(CurrentWorker);
    }

    // Called when a house is upgraded
    public void UpdateHouseCapacityAndWorkers(int addedCapacity, int addedWorkers)
    {
        TotalHousingCapacity += addedCapacity;
        CurrentWorker += addedWorkers;
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
    }

    public void ReduceWorker(int val)
    {
        if (val <= 0 || CurrentWorker <= 0)
            return;

        int workersToRemove = Mathf.Min(val, CurrentWorker);

        for (int i = 0; i < workersToRemove; i++)
        {
            StructureWorkersHouse occupiedHouse =
                workerHouses.Find(h => h != null && h.CurrentWorkers > 0);

            if (occupiedHouse != null) 
                occupiedHouse.RemoveWorkerDueToDeath();
        }

        CurrentWorker -= workersToRemove;
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
        EvtOnWorkerReduced?.Invoke(CurrentWorker);
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