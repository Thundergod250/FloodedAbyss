using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    Health,
    Stamina,
    Oxygen,
    ApexPump
}

[CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Upgrades/Upgrade Configuration")]
public class UpgradeConfigSO : ScriptableObject
{
    [System.Serializable]
    public class UpgradeData
    {
        public UpgradeType type;
        public string title;
        public Sprite icon;
        public ResourceType costResource = ResourceType.Stone;
        public int baseCost = 10;
        public float costMultiplierPerLevel = 1.5f;
        public float baseValue = 100f;
        public float valueIncreasePerLevel = 25f;
        public int maxLevel = 10;
    }

    [SerializeField] private List<UpgradeData> upgrades = new();

    public IReadOnlyList<UpgradeData> Upgrades => upgrades;

    public UpgradeData GetUpgradeData(UpgradeType type)
    {
        return upgrades.Find(u => u.type == type);
    }

    public int GetCostForLevel(UpgradeType type, int currentLevel)
    {
        var data = GetUpgradeData(type);
        if (data == null) return 0;
        return Mathf.RoundToInt(data.baseCost * Mathf.Pow(data.costMultiplierPerLevel, currentLevel));
    }
}