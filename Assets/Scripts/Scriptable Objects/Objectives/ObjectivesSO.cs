using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Objective", menuName = "Game/Objectives/Objective")]
public class ObjectivesSO : ScriptableObject
{
    public enum ObjectiveType
    {
        BuildStructure,
        OpenUI,
        GatherResource,
        ActivatePump,
        DepositResource,
        RecruitWorker,
        AssignWorkers
    }

    [System.Serializable]
    public class ResourceRequirement
    {
        public ResourceDataSO resource;
        [Min(1)] public int requiredAmount = 5;
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

        [Header("Deposit Resource Objective")]
        public List<ResourceRequirement> resourcesToDeposit = new List<ResourceRequirement>();

        [Header("UI Objective")]
        public string requiredUIName;

        [Header("Resource Objective")]
        public List<ResourceRequirement> resourcesToGather =
            new List<ResourceRequirement>();
        
        [Header("Assign Workers Objective")]
        public StructureDataSO structureToAssignWorkers;
        [Min(1)] public int requiredWorkers = 1;
    }

    [Header("Quest Information")]
    [TextArea(2, 4)]
    public string objectiveDescription;

    [Header("Objectives (In Order)")]
    public List<BuildingObjective> objectives =
        new List<BuildingObjective>();
}
