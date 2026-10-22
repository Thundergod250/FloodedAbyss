using UnityEngine;
using UnityEngine.Events;

public class WorkerManager : MonoBehaviour
{
    [field: SerializeField] public int CurrentWorker { get; private set; } = 1;
    [field: SerializeField] public int HomelessWorker { get; private set; } = 1; // Starts with 1 homeless worker
    [field: SerializeField] public int OccupiedHousesCount { get; private set; } = 0;

    [Header("Events")]
    public UnityEvent<int> EvtOnWorkersChanged = new();
    public UnityEvent<int> EvtOnHomelessChanged = new();
    public UnityEvent<int> EvtOnWorkerReduced = new();
    
    private void Start() 
    {
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
        EvtOnHomelessChanged?.Invoke(HomelessWorker);
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

        CurrentWorker = Mathf.Max(0, CurrentWorker - val);

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

    public void RecruitNewWorker()
    {
        CurrentWorker++;
        EvtOnWorkersChanged?.Invoke(CurrentWorker);
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
}