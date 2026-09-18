using UnityEngine;

public class DrainMechanism : Item
{
    [Header("Requirements")]
    [SerializeField] private ResourceType requiredResource = ResourceType.Stone;
    [SerializeField] private int resourceCost = 30;

    private bool isActivated = false;

    public override void Activate()
    {
        if (isActivated) return;

        PlayerResources playerResources = GetPlayerResources();
        if (playerResources == null) return;

        if (playerResources.SpendResource(requiredResource, resourceCost))
        {
            isActivated = true;

            WaterLevel targetWaterLevel = GetWaterLevel();
            if (targetWaterLevel != null)
            {
                targetWaterLevel.LowerWaterOrRaiseBuildings();
                Debug.Log("Water level lowered / buildings raised!");
            }
            else
            {
                Debug.LogWarning("WaterLevel reference missing!");
            }
        }
        else
        {
            Debug.Log($"Not enough {requiredResource} to drain water!");
        }
    }

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }

    private WaterLevel GetWaterLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.waterLevel != null)
        {
            return GameManager.Instance.waterLevel;
        }
        return FindAnyObjectByType<WaterLevel>();
    }
}