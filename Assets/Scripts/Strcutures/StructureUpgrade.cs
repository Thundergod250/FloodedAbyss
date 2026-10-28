using System.Collections.Generic;
using UnityEngine;

public class StructureUpgrade : MonoBehaviour
{
    [Header("Upgrade Info")]
    [SerializeField] private List<StructureDataSO> houseLevels = new List<StructureDataSO>();
    [SerializeField, Range(1, 3)] private int houseLevelVal = 1;

    // NEW: Assign these references from the structure-building script.
    private StructureBuildable owningTerminal;

    // NEW: Store the owning terminal reference.
    public void SetOwningTerminal(StructureBuildable terminal)
    {
        owningTerminal = terminal;
    }

    public bool TryUpgradeHouse()
    {
        StructureWorkersHouse house = GetComponent<StructureWorkersHouse>();

        if (house == null)
        {
            Debug.LogWarning("[StructureUpgrade] StructureWorkersHouse component is missing!", this);
            return false;
        }

        int upgradeIndex = houseLevelVal;

        if (upgradeIndex >= houseLevels.Count)
        {
            Debug.Log("[StructureUpgrade] House is already at max level!");
            return false;
        }

        StructureDataSO upgradeData = houseLevels[upgradeIndex];

        if (upgradeData == null || upgradeData.StructurePrefab == null)
        {
            Debug.LogWarning("[StructureUpgrade] Upgrade data or prefab is missing!", this);
            return false;
        }

        ResourceManager resourceMgr = GameManager.Instance.ResourceManager;

        if (resourceMgr == null)
            return false;

        // CHANGED: Validate all resource costs before instantiating.
        foreach (ResourceCostEntry cost in upgradeData.Costs)
        {
            if (cost.resourceDataSo == null || cost.amount < 0)
                return false;

            if (resourceMgr.GetResourceAmount(cost.resourceDataSo) < cost.amount)
            {
                Debug.Log(
                    $"[StructureUpgrade] Not enough {cost.resourceDataSo.ResourceName} to upgrade!",
                    this
                );
                return false;
            }
        }

        // NEW: Preserve the old house's transform.
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;
        Transform oldParent = transform.parent;
        Vector3 oldScale = transform.localScale;

        GameObject upgradedObject = Instantiate(
            upgradeData.StructurePrefab,
            spawnPosition,
            spawnRotation,
            oldParent
        );

        upgradedObject.transform.localScale = oldScale;

        StructureWorkersHouse upgradedHouse =
            upgradedObject.GetComponent<StructureWorkersHouse>();

        if (upgradedHouse == null)
        {
            Debug.LogError(
                "[StructureUpgrade] Upgraded prefab needs StructureWorkersHouse!",
                upgradedObject
            );

            Destroy(upgradedObject);
            return false;
        }

        // NEW: Transfer terminal ownership through StructureUpgrade.
        StructureUpgrade upgradedUpgrade =
            upgradedObject.GetComponent<StructureUpgrade>();

        if (upgradedUpgrade != null)
        {
            upgradedUpgrade.SetOwningTerminal(owningTerminal);
            upgradedUpgrade.SetHouseLevel(houseLevelVal + 1);
        }

        // NEW: Consume the upgrade costs.
        foreach (ResourceCostEntry cost in upgradeData.Costs)
        {
            if (cost.amount > 0 &&
                !resourceMgr.ConsumeResource(cost.resourceDataSo, cost.amount))
            {
                Debug.LogError("[StructureUpgrade] Failed to consume upgrade costs!", this);
                Destroy(upgradedObject);
                return false;
            }
        }

        // NEW: Refresh the house UI.
        UiWorkersHouse houseUI = GameManager.Instance.uiController.UiWorkersHouse;

        if (houseUI != null && houseUI.IsShowingHouse(house))
        {
            houseUI.Setup(upgradedHouse);
        }

        // CHANGED: Destroy the old house after a successful upgrade.
        Destroy(gameObject);

        return true;
    }

    // NEW: Set the level without modifying StructureWorkersHouse.
    public void SetHouseLevel(int level)
    {
        houseLevelVal = level;
    }

    public int HouseLevelVal => houseLevelVal;
    public List<StructureDataSO> HouseLevels => houseLevels;
}
