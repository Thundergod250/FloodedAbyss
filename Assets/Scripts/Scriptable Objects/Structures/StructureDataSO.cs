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
    public string StructureName => structureName;
    public string Description => description;
    public Sprite StructureIcon => structureIcon;
    public GameObject StructurePrefab => structurePrefab;
    public IReadOnlyList<ResourceRequirement> CostRequirements => costRequirements;
}