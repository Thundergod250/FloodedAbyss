using UnityEngine;
public enum ResourceEventEffect
{
    ReduceResource,
    ReduceWorker,
    ReduceBoth
}
public enum ResourceCondition
{
    Below,
    BelowOrEqual,
    Above,
    AboveOrEqual
}

[CreateAssetMenu(fileName = "ResourceEvent", menuName = "Events/Resource Event")]
public class ResourceEventsUI : ScriptableObject
{
    [Header("Event")]
    [SerializeField] private string eventName;

    [TextArea(2, 5)]
    [SerializeField] private string eventMessage;

    [Header("Trigger")]
    [SerializeField] private ResourceDataSO triggerResource;
    [SerializeField] private ResourceCondition condition = ResourceCondition.BelowOrEqual;


    [SerializeField] private int triggerThreshold;

    [SerializeField] private float minTimerDuration = 60f;
    [SerializeField] private float maxTimerDuration = 240f;

    [Header("Effect")]
    [SerializeField] private ResourceEventEffect effectType;

    [Header("Resource Cost")]
    [SerializeField] private ResourceDataSO resourceToSpend;
    [SerializeField] private int resourceAmount = 1;

    [Header("Worker Effect")]
    [SerializeField] private int reduceWorkerAmount = 1;

    public string EventName => eventName;
    public string EventMessage => eventMessage;
    public ResourceDataSO TriggerResource => triggerResource;

    public ResourceCondition Condition => condition;
    public int TriggerThreshold => triggerThreshold;
    public float MinTimerDuration => minTimerDuration;
    public float MaxTimerDuration => maxTimerDuration;

    public ResourceEventEffect EffectType => effectType;
    public ResourceDataSO ResourceToSpend => resourceToSpend;
    public int ResourceAmount => resourceAmount;
    public int ReduceWorkerAmount => reduceWorkerAmount;
}
