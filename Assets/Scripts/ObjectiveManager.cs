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

    [Header("Debug - Runtime State")]
    [SerializeField] private bool debugAllObjectivesCompleted;

    private readonly Dictionary<ResourceDataSO, int> resourceProgress = new Dictionary<ResourceDataSO, int>();

    public event Action OnObjectiveUpdated;

    private bool IsSequenceValid => questSequence != null && currentQuestIndex >= 0 && currentQuestIndex < questSequence.Count;

    private readonly Dictionary<ResourceDataSO, int> depositProgress = new Dictionary<ResourceDataSO, int>();
    public ObjectivesSO CurrentQuest => IsSequenceValid ? questSequence[currentQuestIndex] : null;

    public ObjectivesSO.BuildingObjective CurrentObjective
    {
        get
        {
            EnsureObjectiveStates();

            ObjectivesSO quest = CurrentQuest;

            if (quest == null)
                return null;

            for (int i = 0; i < quest.objectives.Count; i++)
            {
                if (!objectiveStates[i].completed)
                    return quest.objectives[i];
            }

            return null;
        }
    }

    public int CQI => currentQuestIndex;

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

            if (objective.objectiveType == ObjectivesSO.ObjectiveType.RecruitWorker)
            {
                return "Recruit a worker using the newly built building.";
            }

            if (objective.objectiveType == ObjectivesSO.ObjectiveType.AssignWorkers)
            {
                return $"Assign {objective.requiredWorkers} worker(s) to the quarry";
            }

            return "Complete the required objective";
        }
    }

    private class ObjectiveRuntimeState
    {
        public bool completed;
        public int countProgress;

        public Dictionary<ResourceDataSO, int> gatherProgress =
            new Dictionary<ResourceDataSO, int>();

        public Dictionary<ResourceDataSO, int> depositProgress =
            new Dictionary<ResourceDataSO, int>();
    }

    private List<ObjectiveRuntimeState> objectiveStates = new List<ObjectiveRuntimeState>();

    private ObjectivesSO trackedQuest;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EnsureObjectiveStates();
        //  UpdateResourceEventsState();

        OnObjectiveUpdated?.Invoke();
    }

    private System.Collections.IEnumerator Start()
    {
        yield return null; // Wait one frame for UI initialization

        UpdateResourceEventsState();

        if (GameManager.Instance != null &&
            GameManager.Instance.uiController != null &&
            GameManager.Instance.uiController.GetComponentInChildren<UiTutorial>() != null)
        {
            GameManager.Instance.uiController.GetComponentInChildren<UiTutorial>().OpenUI();

        }
    }

    private void Update()
    {
        debugAllObjectivesCompleted = AllObjectiveCompleted;
    }

    #region Structure Objective
    public void NotifyStructureBuilt(StructureDataSO builtStructure)
    {
        if (builtStructure == null || CurrentQuest == null)
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            var objective = CurrentQuest.objectives[i];
            var state = objectiveStates[i];

            if (state.completed ||
                objective.objectiveType != ObjectivesSO.ObjectiveType.BuildStructure ||
                objective.structureToBuild != builtStructure)
                continue;

            state.countProgress++;

            if (state.countProgress >= Mathf.Max(1, objective.requiredAmount))
                CompleteChecklistItem(i);

            changed = true;
        }

        if (changed)
            CheckQuestCompletion();
    }

    #endregion

    #region UI Objective
    public void NotifyUIOpened(string uiName)
    {
        if (CurrentQuest == null || string.IsNullOrEmpty(uiName))
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            var objective = CurrentQuest.objectives[i];

            if (objectiveStates[i].completed)
                continue;

            if (objective.objectiveType != ObjectivesSO.ObjectiveType.OpenUI ||
                objective.requiredUIName != uiName)
                continue;

            CompleteChecklistItem(i);
            changed = true;
        }

        if (changed)
            CheckQuestCompletion();
    }

    #endregion

    #region Resource Objective
    public void NotifyResourceGathered(ResourceDataSO resource, int amount)
    {
        if (resource == null || amount <= 0 || CurrentQuest == null)
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            var objective = CurrentQuest.objectives[i];
            var state = objectiveStates[i];

            if (state.completed ||
                objective.objectiveType != ObjectivesSO.ObjectiveType.GatherResource ||
                objective.resourcesToGather == null)
                continue;

            int requiredAmount = 0;

            foreach (var requirement in objective.resourcesToGather)
            {
                if (requirement != null && requirement.resource == resource)
                    requiredAmount += Mathf.Max(1, requirement.requiredAmount);
            }

            if (requiredAmount <= 0)
                continue;

            if (!state.gatherProgress.ContainsKey(resource))
                state.gatherProgress[resource] = 0;

            state.gatherProgress[resource] += amount;

            Debug.Log(
                $"[ObjectiveManager] {resource.ResourceName}: " +
                $"{state.gatherProgress[resource]}/{requiredAmount}"
            );

            if (AreAllResourcesGatheredForItem(objective, state))
                CompleteChecklistItem(i);

            changed = true;
        }

        if (changed)
            CheckQuestCompletion();
    }

    private bool AreAllResourcesGatheredForItem(
     ObjectivesSO.BuildingObjective objective,
     ObjectiveRuntimeState state)
    {
        if (objective.resourcesToGather == null)
            return false;

        Dictionary<ResourceDataSO, int> requirements =
            new Dictionary<ResourceDataSO, int>();

        foreach (var requirement in objective.resourcesToGather)
        {
            if (requirement == null || requirement.resource == null ||
                requirement.requiredAmount <= 0)
                continue;

            if (!requirements.ContainsKey(requirement.resource))
                requirements[requirement.resource] = 0;

            requirements[requirement.resource] += requirement.requiredAmount;
        }

        if (requirements.Count == 0)
            return false;

        foreach (var requirement in requirements)
        {
            int gathered = state.gatherProgress.TryGetValue(
                requirement.Key, out int progress) ? progress : 0;

            if (gathered < requirement.Value)
                return false;
        }

        return true;
    }

    #endregion

    #region ApexPump Objective
    public void NotifyPumpActivated(StructureApexPump pump)
    {
        if (pump == null || CurrentQuest == null)
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            if (objectiveStates[i].completed)
                continue;

            if (CurrentQuest.objectives[i].objectiveType !=
                ObjectivesSO.ObjectiveType.ActivatePump)
                continue;

            CompleteChecklistItem(i);
            changed = true;
        }

        if (changed)
            CheckQuestCompletion();
    }

    #endregion

    #region Deposit Objective
    public void NotifyResourceDeposited(ResourceDataSO resource, int amount)
    {
        if (resource == null || amount <= 0 || CurrentQuest == null)
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            var objective = CurrentQuest.objectives[i];
            var state = objectiveStates[i];

            if (state.completed ||
                objective.objectiveType != ObjectivesSO.ObjectiveType.DepositResource ||
                objective.resourcesToDeposit == null)
                continue;

            int requiredAmount = 0;

            foreach (var requirement in objective.resourcesToDeposit)
            {
                if (requirement != null && requirement.resource == resource)
                    requiredAmount += Mathf.Max(1, requirement.requiredAmount);
            }

            if (requiredAmount <= 0)
                continue;

            if (!state.depositProgress.ContainsKey(resource))
                state.depositProgress[resource] = 0;

            state.depositProgress[resource] += amount;

            if (AreAllDepositsCompleteForItem(objective, state))
                CompleteChecklistItem(i);

            changed = true;
        }

        if (changed)
            CheckQuestCompletion();
    }

    private bool AreAllDepositsCompleteForItem(
        ObjectivesSO.BuildingObjective objective,
        ObjectiveRuntimeState state)
    {
        bool foundRequirement = false;

        foreach (var requirement in objective.resourcesToDeposit)
        {
            if (requirement == null ||
                requirement.resource == null ||
                requirement.requiredAmount <= 0)
                continue;

            foundRequirement = true;

            int deposited = state.depositProgress.TryGetValue(
                requirement.resource, out int progress) ? progress : 0;

            if (deposited < requirement.requiredAmount)
                return false;
        }

        return foundRequirement;
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

    #region Recruit Worker Objective

    public void NotifyWorkerRecruited(Structures recruitingBuilding)
    {
        if (recruitingBuilding == null || CurrentQuest == null)
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            if (objectiveStates[i].completed)
                continue;

            if (CurrentQuest.objectives[i].objectiveType !=
                ObjectivesSO.ObjectiveType.RecruitWorker)
                continue;

            CompleteChecklistItem(i);
            changed = true;
        }

        if (changed)
            CheckQuestCompletion();
    }
    #endregion

    #region Assign Worker Objective
    public void NotifyWorkersAssigned(StructureHarvestResource quarry, int assignedWorkers)
    {
        if (quarry == null || CurrentQuest == null)
            return;

        EnsureObjectiveStates();

        bool changed = false;

        for (int i = 0; i < CurrentQuest.objectives.Count; i++)
        {
            var objective = CurrentQuest.objectives[i];

            if (objectiveStates[i].completed ||
                objective.objectiveType != ObjectivesSO.ObjectiveType.AssignWorkers)
                continue;

            if (assignedWorkers >= objective.requiredWorkers)
            {
                CompleteChecklistItem(i);
                changed = true;
            }
        }

        if (changed)
            CheckQuestCompletion();
    }

    #endregion

    #region Objective Progress

    private void CompleteChecklistItem(int index)
    {
        if (index < 0 || index >= objectiveStates.Count)
            return;

        objectiveStates[index].completed = true;

        Debug.Log(
            $"[ObjectiveManager] Completed checklist item: " +
            $"{GetChecklistItemText(index)}"
        );
    }

    private void CheckQuestCompletion()
    {
        EnsureObjectiveStates();

        if (CurrentQuest == null)
            return;

        bool allCompleted = objectiveStates.Count > 0 &&
            objectiveStates.TrueForAll(state => state.completed);

        if (allCompleted)
        {
            Debug.Log("[ObjectiveManager] Quest completed!");

            currentQuestIndex++;
            currentObjectiveIndex = 0;
            currentObjectiveProgress = 0;

            trackedQuest = null;
            objectiveStates.Clear();

            EnsureObjectiveStates();

            if (AllObjectiveCompleted)
                Debug.Log("[ObjectiveManager] All quests completed!");

            UpdateResourceEventsState();
        }

        OnObjectiveUpdated?.Invoke();
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

                if (objective.objectiveType == ObjectivesSO.ObjectiveType.RecruitWorker)
                {
                    return;
                }

                if (objective.objectiveType == ObjectivesSO.ObjectiveType.AssignWorkers)
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

    private void UpdateResourceEventsState()
    {
        bool tutorialStillRunning = !AllObjectiveCompleted;

        UIEventManager eventManager = GameManager.Instance.uiController.GetComponent<UIEventManager>();

        WaterForecast waterForecast = GameManager.Instance.uiController.GetComponentInChildren<WaterForecast>();

        if (eventManager != null)
        {
            eventManager.SetTutorialPaused(tutorialStillRunning);
        }
        else
        {
            Debug.LogError(
                "[ObjectiveManager] UIEventManager not found in the scene!");
        }

        if (waterForecast != null)
        {
            waterForecast.SetTutorialPaused(tutorialStillRunning);
        }
        else
        {
            Debug.LogError(
                "[ObjectiveManager] WaterForecast not found in the scene!");
        }

        Debug.Log(
            $"[ObjectiveManager] Tutorial running: {tutorialStillRunning}");
    }

    private bool HasValidResourceRequirements(ObjectivesSO.BuildingObjective objective)
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

    private void EnsureObjectiveStates()
    {
        ObjectivesSO quest = CurrentQuest;

        if (quest == trackedQuest &&
            objectiveStates.Count == (quest != null && quest.objectives != null
                ? quest.objectives.Count
                : 0))
        {
            return;
        }

        trackedQuest = quest;
        objectiveStates.Clear();

        if (quest == null || quest.objectives == null)
            return;

        for (int i = 0; i < quest.objectives.Count; i++)
        {
            objectiveStates.Add(new ObjectiveRuntimeState());
        }
    }

    public int ObjectiveCount
    {
        get
        {
            EnsureObjectiveStates();
            return objectiveStates.Count;
        }
    }

    public bool IsChecklistItemCompleted(int index)
    {
        EnsureObjectiveStates();

        if (index < 0 || index >= objectiveStates.Count)
            return false;

        return objectiveStates[index].completed;
    }

    public string GetChecklistItemText(int index)
    {
        ObjectivesSO quest = CurrentQuest;

        if (quest == null ||
            quest.objectives == null ||
            index < 0 ||
            index >= quest.objectives.Count)
            return "";

        var objective = quest.objectives[index];

        if (!string.IsNullOrEmpty(objective.objectiveDescription))
            return objective.objectiveDescription;

        return "Complete objective";
    }

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
