using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    private const string DestructionSFXName = "SFXDestruction"; // Swap to "Destruction" if keys resolve against sounds[] entries

    // Dictionary mapping StructureDataSO reference -> Count owned
    private Dictionary<StructureDataSO, int> ownedStructuresCount = new();

    // ── Upgrade suppression ────────────────────────────────────────────
    // Set to true by StructureWorkersHouse.DestroyForUpgrade() (or any other
    // "destroy but treat as upgrade" path). The next UnregisterStructure call
    // will skip the destruction SFX and reset the flag.
    //
    // Frame-expiry: if the flag isn't consumed within the same frame, we
    // treat it as stale and don't suppress the next unrelated unregister.
    [System.NonSerialized] private bool suppressNextDestructionSFX = false;
    [System.NonSerialized] private int suppressFrame = -1;

    public void SuppressNextDestructionSFX()
    {
        suppressNextDestructionSFX = true;
        suppressFrame = Time.frameCount;
    }

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

        if (!ownedStructuresCount.TryGetValue(structureData, out int count))
            return;

        ownedStructuresCount[structureData] = Mathf.Max(0, count - 1);

        // Resolve whether this unregister is an upgrade suppression.
        // Always consume the flag + clear the frame stamp so it can't leak.
        bool suppress = suppressNextDestructionSFX && Time.frameCount == suppressFrame;
        suppressNextDestructionSFX = false;
        suppressFrame = -1;

        bool shouldPlay = playSound && !suppress;

        if (shouldPlay &&
            GameManager.Instance != null &&
            GameManager.Instance.SoundManager != null)
        {
            GameManager.Instance.SoundManager.Play(DestructionSFXName);
        }

        Debug.Log($"[BuildingManager] Unregistered structure: {structureData.StructureTitle}. Total owned: {ownedStructuresCount[structureData]}{(suppress ? " [upgrade - SFX suppressed]" : "")}");
    }

    public int GetOwnedCount(StructureDataSO structureData)
    {
        if (structureData == null) return 0;
        return ownedStructuresCount.TryGetValue(structureData, out int c) ? c : 0;
    }
}