using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    // Dictionary mapping StructureDataSO reference -> Count owned
    private Dictionary<StructureDataSO, int> ownedStructuresCount = new();

    public void RegisterStructure(StructureDataSO structureData)
    {
        if (structureData == null) return;
        if (ownedStructuresCount.ContainsKey(structureData))
            ownedStructuresCount[structureData]++;
        else
            ownedStructuresCount[structureData] = 1;

        Debug.Log($"[BuildingManager] Registered structure: {structureData.StructureTitle}. Total owned: {ownedStructuresCount[structureData]}");
    }

    public void UnregisterStructure(StructureDataSO structureData)
    {
        if (structureData == null) return;
        if (ownedStructuresCount.ContainsKey(structureData))
        {
            ownedStructuresCount[structureData] = Mathf.Max(0, ownedStructuresCount[structureData] - 1);
            Debug.Log($"[BuildingManager] Unregistered structure: {structureData.StructureTitle}. Total owned: {ownedStructuresCount[structureData]}");
        }
    }

    public int GetOwnedCount(StructureDataSO structureData)
    {
        if (structureData == null) return 0;
        if (ownedStructuresCount.TryGetValue(structureData, out int count))
            return count;
        return 0;
    }
}