using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveNameText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;

    [Header("Checklist References")]
    [SerializeField] private Transform checklistParent;
    [SerializeField] private UIObjectiveChecklist checklistItemPrefab;

    private ObjectiveManager subscribedManager;

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
        if (subscribedManager == null &&
            ObjectiveManager.Instance != null)
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

            ClearChecklist();
            return;
        }

        ObjectivesSO quest = manager.CurrentQuest;

        if (manager.AllObjectiveCompleted || quest == null)
        {
            if (objectiveNameText != null)
                objectiveNameText.text = "All Objectives Completed";

            if (objectiveDescriptionText != null)
                objectiveDescriptionText.text = "You've completed every quest!";

            ClearChecklist();
            return;
        }

        if (objectiveNameText != null)
            objectiveNameText.text = quest.objectiveName;

        if (objectiveDescriptionText != null)
            objectiveDescriptionText.text = "";

        // Rebuild the checklist once.
        RefreshChecklist(manager);
    }

    private void ClearChecklist()
    {
        if (checklistParent == null)
            return;

        foreach (Transform child in checklistParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void RefreshChecklist(ObjectiveManager manager)
    {
        // Clear previous checklist items.
        foreach (Transform child in checklistParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < manager.ObjectiveCount; i++)
        {
            UIObjectiveChecklist item =
                Instantiate(checklistItemPrefab, checklistParent);

            item.Setup(
                manager.GetChecklistItemText(i),
                manager.IsChecklistItemCompleted(i)
            );
        }
    }
}