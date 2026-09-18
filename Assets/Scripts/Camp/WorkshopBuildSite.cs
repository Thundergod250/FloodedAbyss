using UnityEngine;

public class WorkshopBuildSite : Item
{
    [Header("Resource Requirements")]
    [SerializeField] private ResourceType requiredResource = ResourceType.Stone;
    [SerializeField] private int resourceCost = 100;

    [Header("Workshop To Spawn")]
    [SerializeField] private GameObject workshopPrefab;
    [SerializeField] private Transform spawnLocation;

    private bool isBuilt = false;

    public override void Activate()
    {
        if (isBuilt) return;

        TryBuildWorkshop();
    }

    private void TryBuildWorkshop()
    {
        PlayerResources playerResources = GetPlayerResources();

        if (playerResources == null)
        {
            Debug.LogWarning("PlayerResources reference missing!");
            return;
        }

        if (playerResources.SpendResource(requiredResource, resourceCost))
        {
            BuildWorkshop();
        }
        else
        {
            Debug.Log($"Not enough {requiredResource} for Workshop! You need {resourceCost}.");
        }
    }

    private void BuildWorkshop()
    {
        isBuilt = true;

        GameObject constructedWorkshop = null;

        if (workshopPrefab != null)
        {
            Vector3 targetPosition = spawnLocation != null ? spawnLocation.position : transform.position;
            Quaternion targetRotation = spawnLocation != null ? spawnLocation.rotation : transform.rotation;

            constructedWorkshop = Instantiate(workshopPrefab, targetPosition, targetRotation);
        }

        Debug.Log("Workshop successfully built!");

        if (constructedWorkshop != null)
        {
            CraftingBench bench = constructedWorkshop.GetComponentInChildren<CraftingBench>(true);
            if (bench != null)
            {
                bench.gameObject.SetActive(true);
            }
        }

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