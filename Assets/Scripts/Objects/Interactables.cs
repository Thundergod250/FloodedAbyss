using UnityEngine;
using UnityEngine.Events;

public class Interactables : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private UnityEvent onInteract;

    private Item item;
    private PlayerResources playerResources;

    private void Awake()
    {
        item = GetComponent<ItemPickup>();
    }

    public virtual void Interact()
    {
        onInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");

        if (item != null)
        {
            item.GainPlayerReference(playerResources);
            item.Activate();
        }
    }

    public void GainPlayerReference(PlayerResources resourceScript)
    {
        playerResources = resourceScript;
    }
}