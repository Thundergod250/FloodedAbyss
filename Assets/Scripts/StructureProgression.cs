using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureProgression : MonoBehaviour
{
    [System.Serializable]
    public class UnlockRequirement
    {
        public StructureDataSO structureToUnlock;

        [Header("Building Requirement")]
        public StructureDataSO requiredStructure;

        [Header("Water Level Requirement")]
        public bool requireWaterLevel = true;
        public float maxWaterHeightToUnlock = 100f;

        [Header("Pump Requirement")]
        public bool requireActivePump = false;
    }

    [Header("Starting Structure")]
    [SerializeField] private StructureDataSO startingStructure;

    [Header("Unlock Requirements")]
    [SerializeField]
    private List<UnlockRequirement> unlockRequirements = new List<UnlockRequirement>();

    [Header("Debug")]
    [SerializeField]
    private List<StructureDataSO> builtStructures
        = new List<StructureDataSO>();

    private HashSet<StructureDataSO> builtSet;
    private StructureApexPump apexPump;
    private WaterLevel waterLevel;


    private void Awake()
    {
        builtSet = new HashSet<StructureDataSO>(builtStructures);

        waterLevel = GameManager.Instance.WaterLevel.GetComponent<WaterLevel>();
        apexPump = GameManager.Instance.ApexPump.GetComponent<StructureApexPump>();
    }

    public bool IsUnlocked(StructureDataSO structure)
    {
        if (structure == null)
            return false;

        if (structure == startingStructure)
            return true;

        UnlockRequirement requirement = unlockRequirements.Find(
            x => x.structureToUnlock == structure
        );

        if (requirement == null)
            return false;

        // Check prerequisite building.
        if (requirement.requiredStructure != null &&
            !builtSet.Contains(requirement.requiredStructure))
        {
            return false;
        }

        // Check water height.
        if (requirement.requireWaterLevel)
        {
            if (waterLevel == null)
                return false;

            if (waterLevel.CurrentWaterHeight >
                requirement.maxWaterHeightToUnlock)
            {
                return false;
            }
        }

        // NEW: Check that the Apex Pump is active.
        if (requirement.requireActivePump)
        {
            if (apexPump == null || !apexPump.IsPumpActive)
                return false;
        }

        return true;
    }

    public void NotifyStructureBuilt(StructureDataSO structure)
    {
        if (structure == null)
            return;

        if (builtSet.Add(structure))
        {
            builtStructures.Add(structure);
            Debug.Log($"[StructureProgression] Built: {structure.name}");
        }
    }

    public bool HasBuilt(StructureDataSO structure)
    {
        return structure != null && builtSet.Contains(structure);
    }
}
