using UnityEngine;

public class UIOverworld_HouseUpgradeNotif : MonoBehaviour
{
    [SerializeField] private GameObject exclamationIndicator;
    private StructureUpgrade structureUpgrade;

    private void Awake() => structureUpgrade = GetComponent<StructureUpgrade>();

    private void Start() => SetIndicatorActive(false);

    private void Update() => CheckUpgradeAffordability();

    private void CheckUpgradeAffordability()
    {
        if (GameManager.Instance == null || GameManager.Instance.ResourceManager == null || structureUpgrade == null)
        {
            SetIndicatorActive(false);
            return;
        }

        // houseLevelVal is 1-based (1 = Level 1), so the index for the *next* level is houseLevelVal itself 
        // (e.g., if currently Level 1, next index is 1, which corresponds to houseLevels[1] for Level 2).
        int nextLevelIndex = structureUpgrade.HouseLevelVal; 
        var houseLevels = structureUpgrade.HouseLevels;

        // Check if max level is already reached
        if (houseLevels == null || nextLevelIndex >= houseLevels.Count)
        {
            SetIndicatorActive(false);
            return;
        }

        StructureDataSO nextUpgradeData = houseLevels[nextLevelIndex];
        if (nextUpgradeData == null || nextUpgradeData.Costs == null || nextUpgradeData.Costs.Count == 0)
        {
            SetIndicatorActive(false);
            return;
        }

        var resourceMgr = GameManager.Instance.ResourceManager;
        bool canAffordAll = true;

        // Check if player has enough for ALL required costs for the next upgrade
        foreach (ResourceCostEntry cost in nextUpgradeData.Costs)
        {
            if (cost.resourceDataSo == null) continue;

            int playerHas = resourceMgr.GetResourceAmount(cost.resourceDataSo);
            if (playerHas < cost.amount)
            {
                canAffordAll = false;
                break;
            }
        }

        SetIndicatorActive(canAffordAll);
    }

    private void SetIndicatorActive(bool isActive)
    {
        if (exclamationIndicator != null && exclamationIndicator.activeSelf != isActive) 
            exclamationIndicator.SetActive(isActive);
    }
}
