using UnityEngine;

public class ItemBuildable : Item
{
    [System.Serializable]
    public struct ResourceCost
    {
        public ResourceType resourceType;
        public int amount;
    }

    [SerializeField] private PlayerResources playerResources; 
    
    [Header("Build Requirements")]
    [SerializeField] private ResourceCost requiredResource;
    [SerializeField] private GameObject prefabToSpawn;

    public override void Activate()
    {
        if (playerResources == null)
        {
            Debug.Log("PlayerResources is null on ItemBuildable");
            return;
        }

        if (playerResources == null)
        {
            Debug.LogError("PlayerResources not found in the scene!");
            return;
        }

        if (playerResources.SpendResource(requiredResource.resourceType, requiredResource.amount))
        {
            Debug.Log($"Successfully built using {requiredResource.amount} {requiredResource.resourceType}");

        if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, transform.position, transform.rotation);
            }
        
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"Cannot build: Missing {requiredResource.amount} {requiredResource.resourceType}");
        }
    }
}