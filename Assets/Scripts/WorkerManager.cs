using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WorkerManager : MonoBehaviour
{
    [field: SerializeField, HideInInspector] public int CurrentWorker { get; private set; } = 1; // Purely for UI display

    [Header("Worker Pools")]
    [SerializeField] private int unhousedWorkers = 1; // Editable in inspector, defaults to 1 immortal baseline
    [SerializeField, HideInInspector] private int housedWorkers = 0;     // Workers living in active houses
    [SerializeField, HideInInspector] private int assignedWorkers = 0;   // Workers currently assigned to workplaces

    [field: SerializeField] public int TotalHousingCapacity { get; private set; } = 0;

    [Header("Events")]
    public UnityEvent<int> EvtOnWorkersChanged = new();
    public UnityEvent<int> EvtOnWorkerReduced = new();

    private List<StructureWorkersHouse> workerHouses = new();
    private List<IWorkerUser> activeWorkplaces = new();

    private void Awake() => RecalculateUI();

    private void RecalculateUI()
    {
        // CurrentWorker is now exclusively for UI use: Total alive workers minus what's currently working
        int totalAliveWorkers = unhousedWorkers + housedWorkers;
        CurrentWorker = Mathf.Max(0, totalAliveWorkers - assignedWorkers);
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
    }

    // Called when a new house is built or upgraded
    public void RegisterHouse(int capacity, int initialWorkers)
    {
        TotalHousingCapacity += capacity;
        housedWorkers += initialWorkers;
        RecalculateUI();
    }

    // Called when a house is destroyed or submerged
    public void UnregisterHouse(int capacity, int housedWorkersInHouse)
    {
        TotalHousingCapacity = Mathf.Max(0, TotalHousingCapacity - capacity);

        // House destruction ONLY kills housed workers. The unhoused worker is untouched!
        int actualHousedKilled = Mathf.Min(housedWorkersInHouse, housedWorkers);
        housedWorkers = Mathf.Max(0, housedWorkers - actualHousedKilled);

        // If active assignments exceed total alive workers, shut down workplaces to resolve deficit
        int totalAliveWorkers = unhousedWorkers + housedWorkers;
        int deficit = assignedWorkers - totalAliveWorkers;
        if (deficit > 0)
        {
            ResolveWorkerDeficit(deficit);
        }

        RecalculateUI();
        if (actualHousedKilled > 0) 
            EvtOnWorkerReduced?.Invoke(CurrentWorker);
    }

    // Priority Check when assigning work: Housed workers first, then Unhoused
    public bool TryConsumeWorkersForLabor(int laborCost)
    {
        int totalAvailable = (unhousedWorkers + housedWorkers) - assignedWorkers;
        if (totalAvailable < laborCost) return false;

        assignedWorkers += laborCost;
        RecalculateUI();
        return true;
    }

    // Returning workers when a workplace stops naturally
    public void ReturnWorkersFromLabor(int laborCost)
    {
        assignedWorkers = Mathf.Max(0, assignedWorkers - laborCost);
        RecalculateUI();
    }

    private void ResolveWorkerDeficit(int deficit)
    {
        while (deficit > 0 && activeWorkplaces.Count > 0)
        {
            int randomIndex = Random.Range(0, activeWorkplaces.Count);
            IWorkerUser workplace = activeWorkplaces[randomIndex];

            if (workplace != null)
            {
                int cost = workplace.LaborCost;
                deficit -= cost;
                assignedWorkers = Mathf.Max(0, assignedWorkers - cost);
                workplace.ForceStopByWorkerShortage(); 
                activeWorkplaces.RemoveAt(randomIndex);
            }
            else
                activeWorkplaces.RemoveAt(randomIndex);
        }
    }

    public void ReduceWorker(int val)
    {
        if (val <= 0 || housedWorkers <= 0)
            return;

        int workersToRemove = Mathf.Min(val, housedWorkers);
        housedWorkers -= workersToRemove;

        for (int i = 0; i < workersToRemove; i++)
        {
            StructureWorkersHouse occupiedHouse =
                workerHouses.Find(h => h != null && h.CurrentWorkers > 0);

            if (occupiedHouse != null) 
                occupiedHouse.RemoveWorkerDueToDeath();
        }

        RecalculateUI();
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