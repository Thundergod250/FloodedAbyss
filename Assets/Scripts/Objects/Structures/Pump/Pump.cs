using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Pump : Item
{
    [Header("UltraPump")]
    public UltraPump ultraPump;

    [Header("Resources")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int currentResources;
    [SerializeField] private int resourceToGive;
    [SerializeField] private int resourceDrain;
    [SerializeField] private int waterLevelReduc;
    [SerializeField] private float drainInterval = 5f; 
    [SerializeField] private float maximumWaterReduction = 10f;

    public float MaximumWaterReduction => maximumWaterReduction;

    private bool hasResources;
    private bool stopDrain;
    private float drainTimer;

    [Header("UI")]
    [SerializeField] private PumpUI pumpUI;

    [SerializeField] private bool isActive;

    private PlayerResources playerResources;

    public bool IsActive => isActive;
    public bool HasResources => currentResources > 0;
    public int ResourceDrainPerSecond => resourceDrain;
    public ResourceType ResourceType => resourceType;
    public float WaterAdjustment => waterLevelReduc;

    public void SetActive(bool value)
    {
        isActive = value;
    }

    private void Start()
    {
        playerResources =
            GameManager.Instance.playerController
                .GetComponent<PlayerResources>();

        resourceToGive = 0;

        pumpUI.gameObject.SetActive(false);
    }

    public override void Activate()
    {
        resourceToGive = 0;

        pumpUI.gameObject.SetActive(true);

        pumpUI.resourceTypeText.text = resourceType.ToString();
        pumpUI.currentResourcesText.text = currentResources.ToString();
        pumpUI.resourcesToGiveText.text = resourceToGive.ToString();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        hasResources = currentResources > 0;

        if (!isActive || stopDrain || !hasResources)
        {
            drainTimer = 0f;
            return;
        }

        drainTimer += Time.deltaTime;

        if (drainTimer >= drainInterval)
        {
            drainTimer = 0f;
            DrainResources();
        }
    }

    public void IncreaseResourceToGive(int amountToGive)
    {
        resourceToGive += amountToGive;
        UpdateText();
    }

    public void DecreaseResourceToGive(int amountToReduce)
    {
        resourceToGive -= amountToReduce;

        if (resourceToGive < 0)
        {
            resourceToGive = 0;
        }

        UpdateText();
    }

    public void ConfirmResources()
    {
        if (resourceToGive <= 0)
            return;

        if (playerResources.SpendResource(resourceType, resourceToGive))
        {
            currentResources += resourceToGive;
            resourceToGive = 0;

            UpdateText();
        }
    }

    public void UpdateText()
    {
        pumpUI.currentResourcesText.text = currentResources.ToString();
        pumpUI.resourcesToGiveText.text = resourceToGive.ToString();
    }

    public void DrainResources()
    {
        if (stopDrain)
            return;

        currentResources -= resourceDrain;
        currentResources = Mathf.Max(0, currentResources);

        UpdateText();

        if (currentResources <= 0)
        {
            isActive = false;

            if (ultraPump != null)
            {
                ultraPump.PumpBecameInactive(this);
            }
        }
    }
}
