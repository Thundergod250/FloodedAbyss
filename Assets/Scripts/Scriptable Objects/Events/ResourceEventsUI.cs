using UnityEngine;

[CreateAssetMenu(fileName = "ResourceEvent", menuName = "Events/Resource Event")]
public class ResourceEventsUI : ScriptableObject
{
    [Header("Event")]
    [SerializeField] private string eventName;

    [TextArea(2, 5)]
    [SerializeField] private string eventMessage;

    [Header("Resource Cost")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int resourceAmount;

    public string EventName => eventName;
    public string EventMessage => eventMessage;
    public ResourceType ResourceType => resourceType;
    public int ResourceAmount => resourceAmount;
}
