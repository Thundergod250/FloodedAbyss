using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillTreeController : MonoBehaviour
{
    [System.Serializable]
    public class SkillTreeEntry
    {
        [Tooltip("Reference to the structure data asset.")]
        public StructureDataSO structureData;

        [Tooltip("Toggle ON to unlock this structure, or OFF to keep it locked.")]
        public bool isUnlocked;
    }

    [Header("Skill Tree Configuration")]
    [Tooltip("List of all structures managed by the Skill Tree and their unlock states.")]
    [SerializeField] private List<SkillTreeEntry> skillTreeEntries = new List<SkillTreeEntry>();

    [Header("Events")]
    [Tooltip("Fired whenever any structure's unlock state changes.")]
    public UnityEvent<StructureDataSO, bool> EvtOnStructureUnlockStateChanged;

    // Runtime collection for fast O(1) unlock state lookups
    private readonly HashSet<string> unlockedStructureIDs = new();

    private void Awake() => InitializeSkillTree();

    public void InitializeSkillTree()
    {
        unlockedStructureIDs.Clear();

        if (skillTreeEntries == null) return;

        foreach (var entry in skillTreeEntries)
        {
            if (entry == null || entry.structureData == null) continue;

            if (entry.isUnlocked) 
                unlockedStructureIDs.Add(entry.structureData.StructureID);
        }
    }
    
    public bool IsUnlocked(StructureDataSO structureData)
    {
        if (structureData == null) return false;
        return IsUnlocked(structureData.StructureID);
    }
    
    public bool IsUnlocked(string structureID)
    {
        if (string.IsNullOrEmpty(structureID)) return true; // Fallback if no ID is set
        return unlockedStructureIDs.Contains(structureID);
    }
    
    public void SetStructureUnlocked(StructureDataSO structureData, bool unlock)
    {
        if (structureData == null) return;

        SkillTreeEntry entry = skillTreeEntries.Find(e => e.structureData == structureData);
        if (entry != null) 
            entry.isUnlocked = unlock;

        bool stateChanged = false;

        if (unlock)
            stateChanged = unlockedStructureIDs.Add(structureData.StructureID);
        else
            stateChanged = unlockedStructureIDs.Remove(structureData.StructureID);

        if (stateChanged)
        {
            Debug.Log($"[SkillTreeController] Structure '{structureData.StructureName}' unlock state changed to: {unlock}");
            EvtOnStructureUnlockStateChanged?.Invoke(structureData, unlock);
        }
    }
    
    public IReadOnlyList<SkillTreeEntry> GetSkillTreeEntries() => 
        skillTreeEntries;

    // Live update when tweaking booleans directly in the Unity Inspector during Play Mode
    private void OnValidate()
    {
        if (Application.isPlaying) 
            InitializeSkillTree();
    }
}