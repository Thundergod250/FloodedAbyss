using UnityEngine;
using System.Collections.Generic;

public class StructureUpgrade : MonoBehaviour
{
    [Header("Upgrade Info")]
    [SerializeField] private List<StructureDataSO> houseLevels = new();
    [SerializeField, Range(1, 3)] private int houseLevelVal = 1;

    private StructureBuildable owningTerminal;

    public void SetOwningTerminal(StructureBuildable terminal) => owningTerminal = terminal;

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

        // 1. Check if player has enough resources
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

        // 2. Consume resources first
        foreach (ResourceCostEntry cost in upgradeData.Costs)
        {
            if (cost.amount > 0 &&
                !resourceMgr.ConsumeResource(cost.resourceDataSo, cost.amount))
            {
                Debug.LogError("[StructureUpgrade] Failed to consume upgrade costs!", this);
                return false;
            }
        }

        // 3. Instantiate the upgraded prefab
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

        StructureUpgrade upgradedUpgrade =
            upgradedObject.GetComponent<StructureUpgrade>();

        if (upgradedUpgrade != null)
        {
            upgradedUpgrade.SetOwningTerminal(owningTerminal);
            upgradedUpgrade.SetHouseLevel(houseLevelVal + 1);
        }

        // 4. Update UI modal focus if it was open for this house
        UiWorkersHouse houseUI = GameManager.Instance.uiController.UiWorkersHouse;

        if (houseUI != null && houseUI.IsShowingHouse(house)) 
            houseUI.Setup(upgradedHouse);

        // 5. Destroy the old building object (its OnDisable unregisters it cleanly)
        Destroy(gameObject);
        return true;
    }

    public void SetHouseLevel(int level) => 
        houseLevelVal = level;

    public int HouseLevelVal => houseLevelVal;
    public List<StructureDataSO> HouseLevels => houseLevels;
}