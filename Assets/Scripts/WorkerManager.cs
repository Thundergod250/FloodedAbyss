using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WorkerManager : MonoBehaviour
{
    [field: SerializeField] public int CurrentWorker { get; private set; } = 1; // Starts at 1 available unhoused worker
    [field: SerializeField] public int TotalHousingCapacity { get; private set; } = 0;

    [Header("Events")]
    public UnityEvent<int> EvtOnWorkersChanged = new();
    public UnityEvent<int> EvtOnWorkerReduced = new();

    private List<StructureWorkersHouse> workerHouses = new();
    private List<IWorkerUser> activeWorkplaces = new();

    private void Awake()
    {
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
    }

    public void SetCurrentWorkers(int amount)
    {
        CurrentWorker = Mathf.Max(0, amount); // Allows 0 when all available workers are deployed to work!
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
        
        // 1. Remove as many dying workers as possible from the idle pool (CurrentWorker)
        int idleToRemove = Mathf.Min(housedWorkers, CurrentWorker);
        CurrentWorker -= idleToRemove;
        
        // 2. If more house workers died than were idle, the rest must have been out working!
        int remainingToKill = housedWorkers - idleToRemove;
        if (remainingToKill > 0)
        {
            ResolveWorkerDeficit(remainingToKill);
        }

        // SAFETY CHECK: Ensure we never drop below 1 total available worker when all housing is gone, 
        // preventing any blockers or soft-locks.
        if (TotalHousingCapacity <= 0 && CurrentWorker < 1)
        {
            CurrentWorker = 1;
        }

        EvtOnWorkersChanged?.Invoke(CurrentWorker);
        if (housedWorkers > 0) 
            EvtOnWorkerReduced?.Invoke(CurrentWorker);
    }

    private void ResolveWorkerDeficit(int deficit)
    {
        // Shut down active workplaces until the worker deficit is cleared
        while (deficit > 0 && activeWorkplaces.Count > 0)
        {
            int randomIndex = Random.Range(0, activeWorkplaces.Count);
            IWorkerUser workplace = activeWorkplaces[randomIndex];

            if (workplace != null)
            {
                // Subtract the labor cost of this workplace (e.g., 2 workers freed/lost)
                deficit -= workplace.LaborCost;
                workplace.ForceStopByWorkerShortage(); // Shuts down workplace and forces workers to return/perish
                activeWorkplaces.RemoveAt(randomIndex);
            }
            else
            {
                activeWorkplaces.RemoveAt(randomIndex);
            }
        }
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

    public void RegisterActiveWorkplace(IWorkerUser workplace)
    {
        if (workplace != null && !activeWorkplaces.Contains(workplace))
            activeWorkplaces.Add(workplace);
    }

    public void UnregisterActiveWorkplace(IWorkerUser workplace)
    {
        if (workplace != null)
            activeWorkplaces.Remove(workplace);
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