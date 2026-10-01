using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Pump : MonoBehaviour
{
    [Header("UltraPump")]
    public UltraPump ultraPump;

    [Header("Resources")]
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private int currentResources;
    [SerializeField] private int resourceToGive;
    [SerializeField] private int resourceDrain;
    [SerializeField] private float drainInterval = 5f; 
    [SerializeField] private float maximumWaterReduction = 10f;
    [SerializeField] private int startingResources;
    [SerializeField] private bool isActive;
    [SerializeField] private Barnacle barnacle; 


    private bool hasResources;
    private bool stopDrain;
    private float drainTimer;



    private PlayerResources playerResources;
    public bool BlockedByBarnacle => barnacle != null && barnacle.gameObject.activeSelf;
    public float MaximumWaterReduction => maximumWaterReduction;
    public bool IsActive => isActive;
    public bool HasResources => currentResources > 0;
    public ResourceType ResourceTypeShared => resourceType;
    public int ResourceToGive => resourceToGive;
    public int ResourceDrainPerSecond => resourceDrain;
    public int StartingResources => startingResources;
    public int CurrentResources => currentResources;
    //public float WaterAdjustment => waterLevelReduc;

    public void SetActive(bool value)
    {
        if (BlockedByBarnacle)
        {
            isActive = false;
            return;
        }

        if (value && !isActive)
        {
            startingResources = currentResources;
        }

        isActive = value;
    }

    private void Start()
    {
        playerResources =
            GameManager.Instance.playerController
                .GetComponent<PlayerResources>();

        resourceToGive = 0;
    }

    private void Update()
    {
        hasResources = currentResources > 0;

        if (!isActive || BlockedByBarnacle || stopDrain || !hasResources)
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
    }

    public void DecreaseResourceToGive(int amountToReduce)
    {
        resourceToGive -= amountToReduce;

        if (resourceToGive < 0)
        {
            resourceToGive = 0;
        }
    }

    public void ConfirmResources()
    {
        if (resourceToGive <= 0)
            return;

        if (playerResources.SpendResource(resourceType, resourceToGive))
        {
            currentResources += resourceToGive;
            resourceToGive = 0;
        }
    }

    public void DrainResources()
    {
        if (stopDrain)
            return;

        currentResources -= resourceDrain;
        currentResources = Mathf.Max(0, currentResources);

        if (currentResources <= 0)
        {
            isActive = false;

            if (ultraPump != null)
            {
                ultraPump.PumpBecameInactive(this);
            }
        }
    }

    #region Colliders

    #endregion
}
