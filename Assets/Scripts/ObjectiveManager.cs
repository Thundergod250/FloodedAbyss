using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("Quest Sequence")]
    [SerializeField]
    private List<ObjectivesSO> questSequence =
        new List<ObjectivesSO>();

    [Header("Debug - Runtime State")]
    [SerializeField] private int currentQuestIndex;
    [SerializeField] private int currentObjectiveIndex;
    [SerializeField] private int currentObjectiveProgress;

    // Tracks gathered amounts separately for each resource.
    private readonly Dictionary<ResourceDataSO, int> resourceProgress =
        new Dictionary<ResourceDataSO, int>();

    public event Action OnObjectiveUpdated;

    private bool IsSequenceValid => questSequence != null && currentQuestIndex >= 0 && currentQuestIndex < questSequence.Count;

    private readonly Dictionary<ResourceDataSO, int> depositProgress = new Dictionary<ResourceDataSO, int>();
    public ObjectivesSO CurrentQuest =>
        IsSequenceValid ? questSequence[currentQuestIndex] : null;

    public ObjectivesSO.BuildingObjective CurrentObjective
    {
        get
        {
            ObjectivesSO quest = CurrentQuest;

            if (quest == null ||
                quest.objectives == null ||
                currentObjectiveIndex < 0 ||
                currentObjectiveIndex >= quest.objectives.Count)
            {
                return null;
            }

            return quest.objectives[currentObjectiveIndex];
        }
    }

    public bool AllObjectiveCompleted => CurrentQuest == null;

    public string CurrentObjectiveText
    {
        get
        {
            var objective = CurrentObjective;

            if (objective == null)
            {
                return AllObjectiveCompleted
                    ? "All quests completed!"
                    : "Quest completed!";
            }

            if (!string.IsNullOrEmpty(objective.objectiveDescription))
                return objective.objectiveDescription;

            if (objective.objectiveType ==
                ObjectivesSO.ObjectiveType.OpenUI)
            {
                return "Open " + objective.requiredUIName;
            }

            if (objective.objectiveType ==
                ObjectivesSO.ObjectiveType.BuildStructure &&
                objective.structureToBuild != null)
            {
                return "Build " + objective.structureToBuild.name;
            }

            if (objective.objectiveType == ObjectivesSO.ObjectiveType.GatherResource)
            {
                if (objective.resourcesToGather != null &&
                    objective.resourcesToGather.Count > 0)
                {
                    var requirements = new List<string>();

                    foreach (var requirement in objective.resourcesToGather)
                    {
                        if (requirement == null ||
                            requirement.resource == null)
                        {
                            continue;
                        }

                        requirements.Add(
                            $"{requirement.requiredAmount} " +
                            requirement.resource.ResourceName
                        );
                    }

                    if (requirements.Count > 0)
                        return "Gather " + string.Join(" and ", requirements);
                }

                return "Gather the required resources";
            }

            if (objective.objectiveType == ObjectivesSO.ObjectiveType.ActivatePump)
            {
                return "Activate the Apex Pump";
            }

            if (objective.objectiveType == ObjectivesSO.ObjectiveType.DepositResource)
            {
                return "Deposit resources into the Apex Pump";
            }

            return "Complete the required objective";
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SkipCompletedObjectives();
        OnObjectiveUpdated?.Invoke();
    }

    #region Structure Objective

    public void NotifyStructureBuilt(StructureDataSO builtStructure)
    {
        var objective = CurrentObjective;

        if (objective == null || builtStructure == null)
            return;

        if (objective.objectiveType !=
            ObjectivesSO.ObjectiveType.BuildStructure)
        {
            return;
        }

        if (objective.structureToBuild != builtStructure)
            return;

        currentObjectiveProgress++;

        Debug.Log(
            $"[ObjectiveManager] {CurrentObjectiveText}: " +
            $"{currentObjectiveProgress}/{objective.requiredAmount}"
        );

        if (currentObjectiveProgress >=
            Mathf.Max(1, objective.requiredAmount))
        {
            AdvanceObjective();
        }
        else
        {
            OnObjectiveUpdated?.Invoke();
        }
    }

    #endregion

    #region UI Objective

    public void NotifyUIOpened(string uiName)
    {
        var objective = CurrentObjective;

        if (objective == null)
            return;

        if (objective.objectiveType !=
            ObjectivesSO.ObjectiveType.OpenUI)
        {
            return;
        }

        if (objective.requiredUIName != uiName)
            return;

        Debug.Log($"[ObjectiveManager] UI opened: {uiName}");

        AdvanceObjective();
    }

    #endregion

    #region Resource Objective

    public void NotifyResourceGathered(
        ResourceDataSO resource,
        int amount)
    {
        var objective = CurrentObjective;

        if (objective == null || resource == null || amount <= 0)
            return;

        if (objective.objectiveType !=
            ObjectivesSO.ObjectiveType.GatherResource)
        {
            return;
        }

        if (objective.resourcesToGather == null ||
            objective.resourcesToGather.Count == 0)
        {
            return;
        }

        // Only accept resources required by this objective.
        bool resourceRequired = objective.resourcesToGather.Exists(
            requirement =>
                requirement != null &&
                requirement.resource == resource &&
                requirement.requiredAmount > 0
        );

        if (!resourceRequired)
            return;

        if (!resourceProgress.ContainsKey(resource))
            resourceProgress[resource] = 0;

        resourceProgress[resource] += amount;

        int requiredAmount = GetRequiredResourceAmount(
            objective, resource);

        int gatheredAmount = resourceProgress[resource];

        Debug.Log(
            $"[ObjectiveManager] Gathering {resource.ResourceName}: " +
            $"{gatheredAmount}/{requiredAmount}"
        );

        // Advance only when every required resource is complete.
        if (AreAllResourcesGathered(objective))
        {
            Debug.Log("[ObjectiveManager] All required resources gathered!");
            AdvanceObjective();
        }
        else
        {
            OnObjectiveUpdated?.Invoke();
        }
    }

    private int GetRequiredResourceAmount(
        ObjectivesSO.BuildingObjective objective,
        ResourceDataSO resource)
    {
        int totalRequired = 0;

        if (objective.resourcesToGather == null)
            return 0;

        foreach (var requirement in objective.resourcesToGather)
        {
            if (requirement == null ||
                requirement.resource != resource)
            {
                continue;
            }

            totalRequired += Mathf.Max(1, requirement.requiredAmount);
        }

        return totalRequired;
    }

    private bool AreAllResourcesGathered(
        ObjectivesSO.BuildingObjective objective)
    {
        if (objective.resourcesToGather == null ||
            objective.resourcesToGather.Count == 0)
        {
            return false;
        }

        bool foundRequirement = false;

        foreach (var requirement in objective.resourcesToGather)
        {
            if (requirement == null ||
                requirement.resource == null ||
                requirement.requiredAmount <= 0)
            {
                continue;
            }

            foundRequirement = true;

            int gathered = resourceProgress.TryGetValue(
                requirement.resource, out int progress)
                ? progress
                : 0;

            if (gathered < requirement.requiredAmount)
                return false;
        }

        return foundRequirement;
    }

    #endregion

    #region ApexPump Objective
    public void NotifyPumpActivated(StructureApexPump pump)
    {
        var objective = CurrentObjective;

        if (objective == null || pump == null)
            return;

        if (objective.objectiveType !=
            ObjectivesSO.ObjectiveType.ActivatePump)
            return;

        Debug.Log("[ObjectiveManager] Apex Pump activated!");

        AdvanceObjective();
    }

    #endregion

    #region Deposit Objective
    public void NotifyResourceDeposited(
    ResourceDataSO resource,
    int amount)
    {
        var objective = CurrentObjective;

        if (objective == null || resource == null || amount <= 0)
            return;

        if (objective.objectiveType !=
            ObjectivesSO.ObjectiveType.DepositResource)
            return;

        if (objective.resourcesToDeposit == null)
            return;

        bool required = objective.resourcesToDeposit.Exists(
            r => r != null &&
                 r.resource == resource &&
                 r.requiredAmount > 0
        );

        if (!required)
            return;

        if (!depositProgress.ContainsKey(resource))
            depositProgress[resource] = 0;

        depositProgress[resource] += amount;

        Debug.Log(
            $"[ObjectiveManager] Deposited {resource.ResourceName}: " +
            $"{depositProgress[resource]}/" +
            $"{GetDepositRequirement(objective, resource)}"
        );

        if (AreAllDepositsComplete(objective))
        {
            Debug.Log("[ObjectiveManager] Deposit objective completed!");
            AdvanceObjective();
        }
        else
        {
            OnObjectiveUpdated?.Invoke();
        }
    }

    private int GetDepositRequirement(
        ObjectivesSO.BuildingObjective objective,
        ResourceDataSO resource)
    {
        int total = 0;

        foreach (var r in objective.resourcesToDeposit)
        {
            if (r != null && r.resource == resource)
                total += Mathf.Max(1, r.requiredAmount);
        }

        return total;
    }

    private bool AreAllDepositsComplete(
        ObjectivesSO.BuildingObjective objective)
    {
        bool foundRequirement = false;

        foreach (var r in objective.resourcesToDeposit)
        {
            if (r == null || r.resource == null || r.requiredAmount <= 0)
                continue;

            foundRequirement = true;

            int deposited = depositProgress.TryGetValue(
                r.resource, out int progress) ? progress : 0;

            if (deposited < r.requiredAmount)
                return false;
        }

        return foundRequirement;
    }
    #endregion

    #region Objective Progress

    private void AdvanceObjective()
    {
        currentObjectiveIndex++;
        currentObjectiveProgress = 0;
        resourceProgress.Clear();

        SkipCompletedObjectives();
        OnObjectiveUpdated?.Invoke();

        if (AllObjectiveCompleted)
        {
            Debug.Log("[ObjectiveManager] All quests completed!");
        }
        else
        {
            Debug.Log(
                $"[ObjectiveManager] New objective: {CurrentObjectiveText}"
            );
        }

        depositProgress.Clear();
    }

    private void SkipCompletedObjectives()
    {
        while (IsSequenceValid)
        {
            ObjectivesSO quest = CurrentQuest;

            if (quest == null ||
                quest.objectives == null ||
                currentObjectiveIndex >= quest.objectives.Count)
            {
                currentQuestIndex++;
                currentObjectiveIndex = 0;
                currentObjectiveProgress = 0;
                resourceProgress.Clear();
                continue;
            }

            var objective = CurrentObjective;

            if (objective != null)
            {
                if (objective.objectiveType ==
                    ObjectivesSO.ObjectiveType.BuildStructure &&
                    objective.structureToBuild != null &&
                    objective.requiredAmount > 0)
                {
                    return;
                }

                if (objective.objectiveType ==
                    ObjectivesSO.ObjectiveType.OpenUI &&
                    !string.IsNullOrEmpty(objective.requiredUIName))
                {
                    return;
                }

                if (objective.objectiveType ==
                    ObjectivesSO.ObjectiveType.GatherResource &&
                    HasValidResourceRequirements(objective))
                {
                    return;
                }

                if (objective.objectiveType == ObjectivesSO.ObjectiveType.ActivatePump)
                {
                    return;
                }

                if (objective.objectiveType == ObjectivesSO.ObjectiveType.DepositResource && objective.resourcesToDeposit != null && objective.resourcesToDeposit.Exists(r => r != null && r.resource != null && r.requiredAmount > 0))
                {
                    return;
                }
            }

            currentObjectiveIndex++;
            currentObjectiveProgress = 0;
            resourceProgress.Clear(); 
            depositProgress.Clear();
        }
    }

    private bool HasValidResourceRequirements(
        ObjectivesSO.BuildingObjective objective)
    {
        if (objective.resourcesToGather == null)
            return false;

        return objective.resourcesToGather.Exists(
            requirement =>
                requirement != null &&
                requirement.resource != null &&
                requirement.requiredAmount > 0
        );
    }

    #endregion

    #region Public Progress Accessors

    public int CurrentProgress => CurrentObjective != null &&
        CurrentObjective.objectiveType ==
        ObjectivesSO.ObjectiveType.GatherResource
            ? GetTotalGatheredResourceProgress()
            : currentObjectiveProgress;

    public int RequiredAmount
    {
        get
        {
            var objective = CurrentObjective;

            if (objective == null)
                return 0;

            if (objective.objectiveType ==
                ObjectivesSO.ObjectiveType.GatherResource)
            {
                if (objective.resourcesToGather == null)
                    return 0;

                int total = 0;

                foreach (var requirement in objective.resourcesToGather)
                {
                    if (requirement == null ||
                        requirement.resource == null)
                    {
                        continue;
                    }

                    total += Mathf.Max(1, requirement.requiredAmount);
                }

                return total;
            }

            return Mathf.Max(1, objective.requiredAmount);
        }
    }

    public int GetResourceProgress(ResourceDataSO resource)
    {
        if (resource == null)
            return 0;

        return resourceProgress.TryGetValue(resource, out int progress)
            ? progress
            : 0;
    }

    private int GetTotalGatheredResourceProgress()
    {
        int total = 0;

        foreach (int amount in resourceProgress.Values)
            total += amount;

        return total;
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}