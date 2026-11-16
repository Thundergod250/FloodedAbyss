using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private const string DestructionSFXName = "SFXDestruction"; // Swap to "Destruction" if your SoundManager keys off the sounds[] array names

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

    /// <summary>
    /// Decrements the owned count for the given structure.
    /// </summary>
    /// <param name="structureData">The structure data to unregister.</param>
    /// <param name="playSound">
    /// True when the structure was genuinely destroyed by gameplay.
    /// Pass false from scene-unload / save-restore / teardown paths
    /// to avoid a phantom destruction SFX on level load or app quit.
    /// </param>
    public void UnregisterStructure(StructureDataSO structureData, bool playSound = true)
    {
        if (structureData == null) return;

        if (ownedStructuresCount.TryGetValue(structureData, out int count))
        {
            ownedStructuresCount[structureData] = Mathf.Max(0, count - 1);

            // ── Destruction SFX (one-shot) ───────────────────────────
            if (playSound &&
                GameManager.Instance != null &&
                GameManager.Instance.SoundManager != null)
            {
                GameManager.Instance.SoundManager.Play(DestructionSFXName);
            }
            // ─────────────────────────────────────────────────────────

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