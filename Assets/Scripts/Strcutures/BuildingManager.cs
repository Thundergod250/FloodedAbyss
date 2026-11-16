using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private static readonly string[] DestructionSFXNames =
    {
        "SFXDestruction (1)",
        "SFXDestruction (2)",
        "SFXDestruction (3)",
        "SFXDestruction (4)"
    };

    private int lastDestructionSfxIndex = -1;

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

    public void UnregisterStructure(StructureDataSO structureData, bool playSound = true)
    {
        if (structureData == null) return;

        if (ownedStructuresCount.TryGetValue(structureData, out int count))
        {
            ownedStructuresCount[structureData] = Mathf.Max(0, count - 1);

            if (playSound &&
                GameManager.Instance != null &&
                GameManager.Instance.SoundManager != null)
            {
                GameManager.Instance.SoundManager.Play(PickDestructionSFX());
            }

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

    private string PickDestructionSFX()
    {
        if (DestructionSFXNames.Length == 0)
            return string.Empty;

        if (DestructionSFXNames.Length == 1)
            return DestructionSFXNames[0];

        int index;
        do
        {
            index = Random.Range(0, DestructionSFXNames.Length);
        }
        while (index == lastDestructionSfxIndex);

        lastDestructionSfxIndex = index;
        return DestructionSFXNames[index];
    }
}