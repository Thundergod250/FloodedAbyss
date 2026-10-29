using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI objectiveNameText;
    [SerializeField] private TextMeshProUGUI objectiveDescriptionText;

    [Header("Food Counter")]
    [SerializeField] private TextMeshProUGUI foodCounterText;
    [SerializeField] private ResourceDataSO foodResource;

    private ObjectiveManager subscribedManager;

    private ResourceManager subscribedResourceManager;


    private void Start()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnEnable()
    {
        TrySubscribe();
        RefreshUI(); 
        RefreshFoodCounter();
    }

    private void OnDisable()
    {
        Unsubscribe();
        UnsubscribeResourceManager(); // NEW
    }

    private void Update()
    {
        if (subscribedManager == null && ObjectiveManager.Instance != null)
        {
            TrySubscribe();
            RefreshUI();
        }

        if (subscribedResourceManager == null)
        {
            TrySubscribeResourceManager();
            RefreshFoodCounter();
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

    private void TrySubscribeResourceManager()
    {
        ResourceManager manager = GameManager.Instance.ResourceManager;

        if (manager == null || subscribedResourceManager == manager)
            return;

        UnsubscribeResourceManager();

        subscribedResourceManager = manager;
        subscribedResourceManager.OnResourceUpdated += OnResourceUpdated;
    }


    private void UnsubscribeResourceManager()
    {
        if (subscribedResourceManager == null)
            return;

        subscribedResourceManager.OnResourceUpdated -= OnResourceUpdated;
        subscribedResourceManager = null;
    }


    private void OnResourceUpdated(ResourceDataSO resource, int amount)
    {
        if (resource == foodResource)
            RefreshFoodCounter();
    }

    private void RefreshFoodCounter()
    {
        if (foodCounterText == null)
            return;

        if (foodResource == null)
        {
            foodCounterText.text = "Food: --";
            return;
        }

        ResourceManager manager = subscribedResourceManager;

        if (manager == null)
        {
            foodCounterText.text = "Food: --";
            return;
        }

        foodCounterText.text =
            $"Food: {manager.GetResourceAmount(foodResource)}";
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
