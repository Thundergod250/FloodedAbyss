using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Objective", menuName = "Game/Objectives/Objective")]
public class ObjectivesSO : ScriptableObject
{
    public enum ObjectiveType
    {
        BuildStructure,
        OpenUI,
        GatherResource
    }

    [System.Serializable]
    public class BuildingObjective
    {
        [Header("Objective")]
        public string objectiveDescription;

        public ObjectiveType objectiveType;

        [Header("Building Objective")]
        public StructureDataSO structureToBuild;
        [Min(1)] public int requiredAmount = 1;

        [Header("UI Objective")]
        public string requiredUIName;

        [Header("Resource Objective")]
        public ResourceDataSO resourceToGather;
        [Min(1)] public int requiredResourceAmount = 5;
    }

    [Header("Quest Information")]
    public string objectiveName;

    [TextArea(2, 4)]
    public string objectiveDescription;

    [Header("Objectives (In Order)")]
    public List<BuildingObjective> objectives =
        new List<BuildingObjective>();
}
