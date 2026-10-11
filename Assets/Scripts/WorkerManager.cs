using UnityEngine;
using UnityEngine.Events;

public class WorkerManager : MonoBehaviour
{
    [field: SerializeField] public int MaximumWorker { get; private set; } = 10;
    [field: SerializeField] public int CurrentWorker { get; private set; } = 1;
    
    [System.Serializable]
    public class WorkerEvent : UnityEvent<int, int> { }

    [Header("Events")]
    public WorkerEvent EvtOnWorkersChanged = new();
    
    private void Start() => EvtOnWorkersChanged?.Invoke(CurrentWorker, MaximumWorker);

    public void SetCurrentWorkers(int amount)
    {
        CurrentWorker = Mathf.Clamp(amount, 0, MaximumWorker);
        EvtOnWorkersChanged?.Invoke(CurrentWorker, MaximumWorker);
    }

    public void SetMaximumWorkers(int amount)
    {
        MaximumWorker = Mathf.Max(0, amount);
        CurrentWorker = Mathf.Min(CurrentWorker, MaximumWorker);
        EvtOnWorkersChanged?.Invoke(CurrentWorker, MaximumWorker);
    }
}