using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [Header("Player Resources")]
    [SerializeField] private PlayerResources playerResources;

    [Header("Resource Events")]
    [SerializeField] private List<ResourceEventsUI> stoneEvents = new();
    [SerializeField] private List<ResourceEventsUI> woodEvents = new();
    [SerializeField] private List<ResourceEventsUI> goldEvents = new();
    [SerializeField] private List<ResourceEventsUI> foodEvents = new();
    [SerializeField] private List<ResourceEventsUI> tinEvents = new();
    [SerializeField] private List<ResourceEventsUI> copperEvents = new();

    [Header("Threshold")]
    [SerializeField] private int resourceThreshold = 100;

    [Header("Event Timer")]
    [SerializeField] private float minTimerDuration = 60f;
    [SerializeField] private float maxTimerDuration = 240f; 

    [Header("Event Cooldown")]
    [SerializeField] private float eventCooldown = 30f;
    private float eventCooldownTimer;
    private bool eventOnCooldown;

    [Header("UI")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private TextMeshProUGUI eventText;

    private ResourceEventsUI resourceEvent;

    private Dictionary<ResourceType, float> resourceTimers = new();
    private Dictionary<ResourceType, float> resourceTimerDurations = new();

    private List<ResourceEventsUI> GetEvents(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Stone:
                return stoneEvents;

            case ResourceType.Wood:
                return woodEvents;

            case ResourceType.Gold:
                return goldEvents;

            case ResourceType.Food:
                return foodEvents;

            case ResourceType.Tin:
                return tinEvents;

            case ResourceType.Copper:
                return copperEvents;

            default:
                return null;
        }
    }

    private void Start()
    {
        eventPanel = GameManager.Instance.uiController.GetComponentInChildren<EventsUI>().panel;
        eventText = GameManager.Instance.uiController.GetComponentInChildren<EventsUI>().eventText;


        eventPanel.SetActive(false);

        playerResources = GameManager.Instance.playerController.GetComponent<PlayerResources>();

        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resourceTimers[type] = 0f;
            resourceTimerDurations[type] = UnityEngine.Random.Range(
                minTimerDuration,
                maxTimerDuration
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
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            int amount = playerResources.GetResource(type);

            if (amount >= resourceThreshold)
            {
                resourceTimers[type] += Time.deltaTime;

                if (resourceTimers[type] >= resourceTimerDurations[type])
                {
                    ShowResourceUI(type);
                    resourceTimers[type] = 0f;

                    resourceTimerDurations[type] = UnityEngine.Random.Range(
                        minTimerDuration,
                        maxTimerDuration
                    );
                }
            }
            else
            {
                resourceTimers[type] = 0f;
            }
        }
    }

    private void ShowResourceUI(ResourceType type)
    {
        if (eventOnCooldown)
            return;

        List<ResourceEventsUI> events = GetEvents(type);

        if (events == null || events.Count == 0)
        {
            Debug.LogWarning($"No events assigned for {type}.");
            return;
        }

        ResourceEventsUI selectedEvent =
            events[UnityEngine.Random.Range(0, events.Count)];

        playerResources.SpendResource(
            selectedEvent.ResourceType,
            selectedEvent.ResourceAmount
        );

        eventPanel.SetActive(true);
        eventText.text = selectedEvent.EventMessage;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        eventOnCooldown = true;
        eventCooldownTimer = eventCooldown;
    }

    public void CloseEvent()
    {
        eventPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
