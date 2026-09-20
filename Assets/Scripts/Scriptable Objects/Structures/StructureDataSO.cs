using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Structure Data", menuName = "Structure Data")]
public class StructureDataSO : ScriptableObject
{
    [System.Serializable]
    public struct ResourceRequirement
    {
        public ResourceType resourceType;
        public int amount;
    }

    [Header("Identification & Skill Tree")]
    [Tooltip("Unique ID used by the Skill Tree / Tech System to check if this structure is unlocked.")]
    [SerializeField] private string structureID;
    
    [Tooltip("Text displayed on the UI card when this structure is locked.")]
    [SerializeField] private string unlockRequirementHint = "Unlocked in Research Table";

    [Header("Structure Data")]
    [SerializeField] private string structureName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite structureIcon;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject structurePrefab;

    [Header("Cost Requirements Data")]
    [SerializeField] private List<ResourceRequirement> costRequirements = new List<ResourceRequirement>();

    // Encapsulated getters for read-only access
    public string StructureID => structureID;
    public string UnlockRequirementHint => unlockRequirementHint;
    public string StructureName => structureName;
    public string Description => description;
    public Sprite StructureIcon => structureIcon;
    public GameObject StructurePrefab => structurePrefab;
    public IReadOnlyList<ResourceRequirement> CostRequirements => costRequirements;

    private void OnValidate()
    {
        // Auto-fills structureID with a clean lowercase string based on the asset name if left blank
        if (string.IsNullOrWhiteSpace(structureID)) 
            structureID = name.ToLower().Replace(" ", "_");
    }
}