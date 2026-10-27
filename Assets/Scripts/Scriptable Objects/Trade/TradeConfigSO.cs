using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TradeConfig", menuName = "Trading/Trade Configuration")]
public class TradeConfigSO : ScriptableObject
{
    [System.Serializable]
    public struct ResourceRate
    {
        public ResourceType resourceType;
        [Tooltip("Base value/tier weight of this resource. (e.g. Wood = 1, Stone = 2, Gold = 5)")]
        public int baseValue;
    }

    [Header("Available Resources in Trading")]
    [SerializeField]
    private List<ResourceType> availableTradeResources = new()
    {
        ResourceType.Wood,
        ResourceType.Stone,
        ResourceType.Copper,
        ResourceType.Iron,
        ResourceType.Gold
    };

    [Header("Resource Base Values")]
    [SerializeField]
    private List<ResourceRate> resourceRates = new()
    {
        new ResourceRate { resourceType = ResourceType.Wood, baseValue = 1 },
        new ResourceRate { resourceType = ResourceType.Stone, baseValue = 2 },
        new ResourceRate { resourceType = ResourceType.Copper, baseValue = 3 },
        new ResourceRate { resourceType = ResourceType.Iron, baseValue = 4 },
        new ResourceRate { resourceType = ResourceType.Gold, baseValue = 5 }
    };

    public IReadOnlyList<ResourceType> AvailableTradeResources => availableTradeResources;

    /// <summary>
    /// Gets the base value for a specific resource type.
    /// </summary>
    public int GetResourceValue(ResourceType type)
    {
        foreach (var rate in resourceRates)
        {
            if (rate.resourceType == type)
                return Mathf.Max(1, rate.baseValue);
        }
        return 1; // Default fallback
    }

    /// <summary>
    /// Calculates the output resource quantity given the input quantity and types.
    /// Formula: (Deposit Amount * Deposit Resource Value) / Receive Resource Value
    /// </summary>
    public int CalculateReceiveAmount(ResourceType depositType, ResourceType receiveType, int depositAmount)
    {
        if (depositType == receiveType || depositAmount <= 0)
            return 0;

        int depositValue = GetResourceValue(depositType);
        int receiveValue = GetResourceValue(receiveType);

        return (depositAmount * depositValue) / receiveValue;
    }
}