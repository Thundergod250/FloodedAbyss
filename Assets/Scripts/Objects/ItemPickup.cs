using UnityEngine;

public class ItemPickup : Item
{
    [Header("Drops")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int amountToDrop;

    private PlayerResources playerResources;

    public override void Activate()
    {
        Debug.Log($"Picked up {gameObject.name}"); 

        if (playerResources != null)
        {
            playerResources.AddResource(resourceType, amountToDrop);
        }

        gameObject.SetActive(false);
    }

    public override void GainPlayerReference(PlayerResources resourceScript)
    {
        playerResources = resourceScript;
    }
}