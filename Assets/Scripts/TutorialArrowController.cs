using UnityEngine;

public class TutorialArrowController : MonoBehaviour
{
    [Header("Arrowdown Instances (Assign from Hierarchy)")]
    [Tooltip("The Arrowdown object attached to the Timber Stationary")]
    public GameObject timberArrow;

    [Tooltip("The Arrowdown object attached to the Fiber Stationary")]
    public GameObject fiberArrow;

    [Tooltip("The Arrowdown object attached to the Omega Pump")]
    public GameObject pumpArrow;

    [Header("Target Structures")]
    public StructureHarvestResource timberHarvest;
    public StructureHarvestResource fiberHarvest;
    public StructureApexPump apexPump;

    [Header("Objective Assets")]
    [Tooltip("Assign 'Objective 1' asset")]
    public ObjectivesSO objective1;

    [Tooltip("Assign 'Objective 2' asset")]
    public ObjectivesSO objective2;

    private void Update()
    {
        // Wait until the ObjectiveManager and its current quest are loaded
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null)
            return;

        ObjectivesSO currentQuest = ObjectiveManager.Instance.CurrentQuest;

        // ---------------------------------------------------------
        // OBJECTIVE 1: Gather 50 Timber and 50 Fiber
        // ---------------------------------------------------------
        if (currentQuest == objective1)
        {
            // Show Timber arrow only if the player isn't currently harvesting (progress bar is hidden)
            if (timberArrow != null && timberHarvest != null)
            {
                timberArrow.SetActive(!timberHarvest.IsWorking);
            }

            // Show Fiber arrow only if the player isn't currently harvesting (progress bar is hidden)
            if (fiberArrow != null && fiberHarvest != null)
            {
                fiberArrow.SetActive(!fiberHarvest.IsWorking);
            }

            // Ensure the pump arrow is hidden during this phase
            if (pumpArrow != null)
            {
                pumpArrow.SetActive(false);
            }
        }
        // ---------------------------------------------------------
        // OBJECTIVE 2: Deposit 50 Fiber into the Pump & Activate it
        // ---------------------------------------------------------
        else if (currentQuest == objective2)
        {
            // Permanently disable the harvesting arrows
            if (timberArrow != null) timberArrow.SetActive(false);
            if (fiberArrow != null) fiberArrow.SetActive(false);

            // Show the pump arrow until the pump is fueled (or activated)
            if (pumpArrow != null && apexPump != null)
            {
                // Once it has fuel or is turned on, disable the arrow
                bool isFueled = apexPump.CurrentFuel > 0 || apexPump.IsPumpActive;
                pumpArrow.SetActive(!isFueled);
            }
        }
        // ---------------------------------------------------------
        // LATER OBJECTIVES: Hide everything
        // ---------------------------------------------------------
        else
        {
            if (timberArrow != null) timberArrow.SetActive(false);
            if (fiberArrow != null) fiberArrow.SetActive(false);
            if (pumpArrow != null) pumpArrow.SetActive(false);
        }
    }
}