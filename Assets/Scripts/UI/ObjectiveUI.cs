using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveNameText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;

    private ObjectiveManager subscribedManager;

    private void Start()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnEnable()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (subscribedManager == null && ObjectiveManager.Instance != null)
        {
            TrySubscribe();
            RefreshUI();
        }
    }

    private void TrySubscribe()
    {
        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null || subscribedManager == manager)
            return;

        Unsubscribe();

        subscribedManager = manager;
        subscribedManager.OnObjectiveUpdated += RefreshUI;
    }

    private void Unsubscribe()
    {
        if (subscribedManager == null)
            return;

        subscribedManager.OnObjectiveUpdated -= RefreshUI;
        subscribedManager = null;
    }

    private void RefreshUI()
    {
        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null)
        {
            if (objectiveNameText != null)
                objectiveNameText.text = "No Objectives";

            if (objectiveDescriptionText != null)
                objectiveDescriptionText.text = "Objective manager not found.";

            return;
        }

        ObjectivesSO quest = manager.CurrentQuest;
        ObjectivesSO.BuildingObjective objective = manager.CurrentObjective;

        if (objectiveNameText != null)
        {
            if (manager.AllObjectiveCompleted)
                objectiveNameText.text = "All Objectives Completed";
            else if (objective != null &&
                     !string.IsNullOrEmpty(objective.objectiveDescription))
                objectiveNameText.text = objective.objectiveDescription;
            else
                objectiveNameText.text = "Current Objective";
        }

        if (objectiveDescriptionText != null)
        {
            if (manager.AllObjectiveCompleted)
                objectiveDescriptionText.text = "You've completed every quest!";
            else if (quest != null)
                objectiveDescriptionText.text = quest.objectiveDescription;
            else
                objectiveDescriptionText.text = "";
        }
    }
}
