using System;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using static ResourceEventsUI;
using Random = UnityEngine.Random;

public class UIEventManager : UiModals
{
    [Header("Resource Events")]
    [SerializeField] private List<ResourceEventsUI> resourceEvents = new();

    [Header("Event Cooldown")]
    [SerializeField] private float eventCooldown = 30f;
    private float eventCooldownTimer;
    private bool eventOnCooldown;

    [Header("UI")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private TextMeshProUGUI eventText;


    private WorkerManager workerManager; 
    private ResourceManager resourceManager;
    private readonly Dictionary<ResourceEventsUI, float> eventTimers = new();
    private readonly Dictionary<ResourceEventsUI, float> eventTimerDurations = new();

    protected override void Initialize()
    {
        workerManager = GameManager.Instance.WorkerManager;
        resourceManager = GameManager.Instance.ResourceManager;

        if (resourceManager == null)
        {
            Debug.LogError("[UIEventManager] ResourceManager is not assigned.", this);
            enabled = false;
            return;
        }

        if (eventText == null)
        {
            Debug.LogError("[UIEventManager] Event Text is not assigned.", this);
            enabled = false;
            return;
        }

        foreach (ResourceEventsUI eventData in resourceEvents)
        {
            if (eventData == null || eventData.TriggerResource == null)
                continue;

            if (eventTimers.ContainsKey(eventData))
                continue;

            eventTimers[eventData] = 0f;
            eventTimerDurations[eventData] = GetRandomTimerDuration(
                eventData.MinTimerDuration,
                eventData.MaxTimerDuration
            );
        }

    }

    private void Update()
    {
        if (eventOnCooldown)
        {
            eventCooldownTimer -= Time.deltaTime;

            if (eventCooldownTimer <= 0f)
            {
                eventCooldownTimer = 0f;
                eventOnCooldown = false;
            }

            return;
        }

        CheckResources();
    }

    private void CheckResources()
    {
        foreach (ResourceEventsUI eventData in resourceEvents)
        {
            if (eventData == null || eventData.TriggerResource == null)
                continue;

            if (!eventTimers.ContainsKey(eventData))
                continue;

            int amount = resourceManager.GetResourceAmount(
                eventData.TriggerResource
            );

            bool conditionMet = eventData.Condition switch
            {
                ResourceCondition.Below =>
                    amount < eventData.TriggerThreshold,

                ResourceCondition.BelowOrEqual =>
                    amount <= eventData.TriggerThreshold,

                ResourceCondition.Above =>
                    amount > eventData.TriggerThreshold,

                ResourceCondition.AboveOrEqual =>
                    amount >= eventData.TriggerThreshold,

                _ => false
            };

            if (!conditionMet)
            {
                eventTimers[eventData] = 0f;
                continue;
            }

            eventTimers[eventData] += Time.deltaTime;

            if (eventTimers[eventData] >= eventTimerDurations[eventData])
            {
                if (!eventOnCooldown)
                {
                    ShowResourceUI(eventData);

                    if (eventOnCooldown)
                    {
                        eventTimers[eventData] = 0f;
                        eventTimerDurations[eventData] =
                            GetRandomTimerDuration(
                                eventData.MinTimerDuration,
                                eventData.MaxTimerDuration
                            );
                    }
                }
            }
        }
    }

    private void ShowResourceUI(ResourceEventsUI selectedEvent)
    {
        if (eventOnCooldown)
            return;

        if (selectedEvent == null)
            return;

        switch (selectedEvent.EffectType)
        {
            case ResourceEventEffect.ReduceResource:
                if (!TrySpendEventResource(selectedEvent))
                    return;
                break;

            case ResourceEventEffect.ReduceWorker:
                if (workerManager == null)
                {
                    Debug.LogWarning(
                        "[UIEventManager] WorkerManager is not assigned.",
                        this
                    );
                    return;
                }

                workerManager.ReduceWorker(selectedEvent.ReduceWorkerAmount);
                break;

            case ResourceEventEffect.ReduceBoth:
                if (workerManager == null)
                {
                    Debug.LogWarning(
                        "[UIEventManager] WorkerManager is not assigned.",
                        this
                    );
                    return;
                }

                if (!TrySpendEventResource(selectedEvent))
                    return;

                workerManager.ReduceWorker(selectedEvent.ReduceWorkerAmount);
                break;
        }

        eventText.text = selectedEvent.EventMessage;

        GameManager.Instance.uiController.OpenModal<UIEventManager>();

        eventOnCooldown = true;
        eventCooldownTimer = eventCooldown;
    }

    private bool TrySpendEventResource(ResourceEventsUI eventData)
    {
        if (eventData == null || resourceManager == null || eventData.ResourceToSpend == null || eventData.ResourceAmount <= 0)
        {
            Debug.LogWarning(
                $"[UIEventManager] Invalid resource cost for '{eventData?.EventName}'.",
                this);

            return false;
        }

        if (eventData.boonORBane == BoonORBane.Boon)
        {
            resourceManager.AddResource(eventData.ResourceToSpend, eventData.ResourceAmount);
        }
        else if (eventData.boonORBane == BoonORBane.Bane)
        {
            resourceManager.ConsumeResource(eventData.ResourceToSpend, eventData.ResourceAmount);
        }

        return true;
    }

    private float GetRandomTimerDuration(float minDuration, float maxDuration)
    {
        float min = Mathf.Max(0f, Mathf.Min(minDuration, maxDuration));
        float max = Mathf.Max(0f, Mathf.Max(minDuration, maxDuration));

        return Random.Range(min, max);
    }

}