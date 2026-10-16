using System;
using System.Collections.Generic;
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

    // NEW: UI can subscribe to this event to refresh quest text.
    public event Action OnObjectiveUpdated;

    private bool IsSequenceValid =>
        questSequence != null &&
        currentQuestIndex >= 0 &&
        currentQuestIndex < questSequence.Count;

    public ObjectivesSO CurrentQuest =>
        IsSequenceValid ? questSequence[currentQuestIndex] : null;

    public ObjectivesSO.BuildingObjective CurrentObjective
    {
        get
        {
            ObjectivesSO quest = CurrentQuest;

            if (quest == null || quest.objectives == null ||
                currentObjectiveIndex < 0 ||
                currentObjectiveIndex >= quest.objectives.Count)
                return null;

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
                return AllObjectiveCompleted
                    ? "All quests completed!"
                    : "Quest completed!";

            if (!string.IsNullOrEmpty(objective.objectiveDescription))
                return objective.objectiveDescription;

            if (objective.objectiveType == ObjectivesSO.ObjectiveType.OpenUI)
            {
                return "Open " + objective.requiredUIName;
            }

            if (objective.structureToBuild != null)
            {
                return "Build " + objective.structureToBuild.name;
            }
            if (objective.objectiveType == ObjectivesSO.ObjectiveType.GatherResource && objective.resourceToGather != null)
            {
                return "Gather " + objective.requiredResourceAmount +
                       " " + objective.resourceToGather.ResourceName;
            }

            return "Build the required structure";
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

    #region Notification When Objective is Accomplished
    public void NotifyStructureBuilt(StructureDataSO builtStructure)
    {
        ObjectivesSO.BuildingObjective objective = CurrentObjective;

        if (objective == null || builtStructure == null)
            return;

        // Only count the building required by the current objective.
        if (objective.structureToBuild != builtStructure)
            return;

        currentObjectiveProgress++;

        Debug.Log(
            $"[QuestManager] {CurrentObjectiveText}: " +
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

    public void NotifyUIOpened(string uiName)
    {
        ObjectivesSO.BuildingObjective objective = CurrentObjective;

        if (objective == null)
            return;

        if (objective.objectiveType != ObjectivesSO.ObjectiveType.OpenUI)
            return;

        if (objective.requiredUIName != uiName)
            return;

        Debug.Log($"[ObjectiveManager] UI opened: {uiName}");

        AdvanceObjective();
    }

    public void NotifyResourceGathered(ResourceDataSO resource, int amount)
    {
        ObjectivesSO.BuildingObjective objective = CurrentObjective;

        if (objective == null || resource == null || amount <= 0)
            return;

        if (objective.objectiveType != ObjectivesSO.ObjectiveType.GatherResource)
            return;

        if (objective.resourceToGather != resource)
            return;

        currentObjectiveProgress += amount;

        Debug.Log(
            $"[ObjectiveManager] Gathering {resource.ResourceName}: " +
            $"{currentObjectiveProgress}/{objective.requiredResourceAmount}"
        );

        if (currentObjectiveProgress >= Mathf.Max(1, objective.requiredResourceAmount))
        {
            AdvanceObjective();
        }
        else
        {
            OnObjectiveUpdated?.Invoke();
        }
    }
    #endregion

    #region ObjectiveGoNext
    private void AdvanceObjective()
    {
        currentObjectiveIndex++;
        currentObjectiveProgress = 0;

        SkipCompletedObjectives();
        OnObjectiveUpdated?.Invoke();

        if (AllObjectiveCompleted)
        {
            Debug.Log("[QuestManager] All quests completed!");
        }
        else
        {
            Debug.Log(
                $"[QuestManager] New objective: {CurrentObjectiveText}"
            );
        }
    }

    private void SkipCompletedObjectives()
    {
        while (IsSequenceValid)
        {
            ObjectivesSO quest = CurrentQuest;

            if (quest == null || quest.objectives == null ||
                currentObjectiveIndex >= quest.objectives.Count)
            {
                currentQuestIndex++;
                currentObjectiveIndex = 0;
                currentObjectiveProgress = 0;
                continue;
            }

            var objective = CurrentObjective;

            if (objective != null)
            {
                if (objective.objectiveType ==
                        ObjectivesSO.ObjectiveType.BuildStructure &&
                    objective.structureToBuild != null)
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
                    objective.resourceToGather != null &&
                    objective.requiredResourceAmount > 0)
                {
                    return;
                }
            }

            currentObjectiveIndex++;
            currentObjectiveProgress = 0;
        }
    }
    #endregion

    public int CurrentProgress => currentObjectiveProgress;

    public int RequiredAmount =>
        CurrentObjective != null
            ? Mathf.Max(1, CurrentObjective.requiredAmount)
            : 0;

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
