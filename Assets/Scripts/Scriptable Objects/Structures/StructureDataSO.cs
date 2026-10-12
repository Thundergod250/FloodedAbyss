using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResourceCostEntry
{
    public ResourceDataSO resourceDataSo;
    public int amount;
}

[CreateAssetMenu(fileName = "NewStructureData", menuName = "Structure Data")]
public class StructureDataSO : ScriptableObject
{
    [field: SerializeField] public string StructureTitle { get; private set; }
    [field: SerializeField, TextArea] public string StructureInfo { get; private set; }
    [field: SerializeField] public Sprite StructureIcon { get; private set; }
    [field: SerializeField] public GameObject StructurePrefab { get; private set; } // Prefab to instantiate in world
    [field: SerializeField] public List<ResourceCostEntry> Costs { get; private set; } = new();
}