using UnityEngine;

public class CampBuildSite : Item
{
    [Header("Resource Requirement")]
    [SerializeField] private ResourceType requiredResource = ResourceType.Stone;
    [SerializeField] private int resourceCost = 20;

    [Header("Camp To Spawn")]
    [SerializeField] private GameObject campPrefab;
    [SerializeField] private Transform spawnLocation;

    private bool isBuilt = false;

    public override void Activate()
    {
        if (isBuilt) return;

        TryBuildCamp();
    }

    private void TryBuildCamp()
    {
        PlayerResources playerResources = GetPlayerResources();

        if (playerResources == null)
        {
            Debug.LogWarning("PlayerResources not found!");
            return;
        }

        if (playerResources.SpendResource(requiredResource, resourceCost))
        {
            BuildCamp();
        }
        else
        {
            Debug.Log($"Not enough {requiredResource}! You need {resourceCost}.");
        }
    }

    private void BuildCamp()
    {
        isBuilt = true;

        if (campPrefab != null)
        {
            Vector3 targetPosition = spawnLocation != null ? spawnLocation.position : transform.position;
            Quaternion targetRotation = spawnLocation != null ? spawnLocation.rotation : transform.rotation;

            Instantiate(campPrefab, targetPosition, targetRotation);
        }

        Debug.Log("Camp constructed successfully!");

        gameObject.SetActive(false);
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