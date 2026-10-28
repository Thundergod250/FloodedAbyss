using UnityEngine;

public class ItemPickup : Item
{
    [Header("Resource Settings")]
    [SerializeField] private ResourceType resourceType = ResourceType.Stone;
    [SerializeField] private int resourceAmount = 1;

    public override void Activate()
    {
        PlayerResources resources = GetPlayerResources();
        if (resources != null)
        {
            resources.AddResource(resourceType, resourceAmount);
            Debug.Log($"Picked up {gameObject.name}: +{resourceAmount} {resourceType}");
        }

        Pool.Destroy(gameObject);
    }

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }
}