using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    // Dictionary mapping Structure Title -> Count owned
    private Dictionary<string, int> ownedStructuresCount = new Dictionary<string, int>();

    public void RegisterStructure(string structureTitle)
    {
        if (string.IsNullOrEmpty(structureTitle)) return;

        if (ownedStructuresCount.ContainsKey(structureTitle))
        {
            ownedStructuresCount[structureTitle]++;
        }
        else
        {
            ownedStructuresCount[structureTitle] = 1;
        }

        Debug.Log($"[BuildingManager] Registered structure: {structureTitle}. Total owned: {ownedStructuresCount[structureTitle]}");
    }

    public void UnregisterStructure(string structureTitle)
    {
        if (string.IsNullOrEmpty(structureTitle)) return;

        if (ownedStructuresCount.ContainsKey(structureTitle))
        {
            ownedStructuresCount[structureTitle] = Mathf.Max(0, ownedStructuresCount[structureTitle] - 1);
            Debug.Log($"[BuildingManager] Unregistered structure: {structureTitle}. Total owned: {ownedStructuresCount[structureTitle]}");
        }
    }

    public int GetOwnedCount(string structureTitle)
    {
        if (string.IsNullOrEmpty(structureTitle)) return 0;

        if (ownedStructuresCount.TryGetValue(structureTitle, out int count))
        {
            return count;
        }
        return 0;
    }
}