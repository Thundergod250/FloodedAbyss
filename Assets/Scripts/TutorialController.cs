using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [Header("Objective Assets")]
    public ObjectivesSO objective1;
    public ObjectivesSO objective2;
    public List<GameObject> currentGlowbox; 
    

    private StructureHarvestResource[] allHarvesters;
    private StructureApexPump[] allPumps;

    // UI Tracking Variables
    private GameObject apexPumpPanel;
    private bool isUIHooked = false;
    private bool clickedResource = false;
    private bool clickedAmount = false;
    private bool clickedActivate = false;

    private void Start()
    {
        // Find all world objects dynamically
        allHarvesters = FindObjectsByType<StructureHarvestResource>(FindObjectsInactive.Exclude);
        allPumps = FindObjectsByType<StructureApexPump>(FindObjectsInactive.Exclude);

        // Find the UI panel immediately so we can disable its arrows if Objective 1 is active
        FindPumpPanel();
    }

    private void Update()
    {
        // Wait until the ObjectiveManager is ready
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null)
            return;

        ObjectivesSO currentQuest = ObjectiveManager.Instance.CurrentQuest;

        bool isObjective1 = (currentQuest == objective1);
        bool isObjective2 = (currentQuest == objective2);

        // --- HANDLE TIMBER & FIBER ARROWS ---
        foreach (var harvester in allHarvesters)
        {
            if (harvester == null) continue;

            Transform arrow = GetArrowChild(harvester.transform);
            if (arrow == null) continue;

            if (isObjective1)
            {
                // Show arrow ONLY if the progress bar is hidden (not working)
                arrow.gameObject.SetActive(!harvester.IsWorking);
            }
            else
            {
                // Permanently disable once Objective 1 is done
                arrow.gameObject.SetActive(false);
            }
        }

        // --- HANDLE PUMP WORLD ARROWS ---
        foreach (var pump in allPumps)
        {
            if (pump == null) continue;

            Transform arrow = GetArrowChild(pump.transform);
            if (arrow == null) continue;

            if (isObjective2)
            {
                // Show arrow UNTIL it is fueled or activated
                bool isFueled = pump.CurrentFuel > 0 || pump.IsPumpActive;
                arrow.gameObject.SetActive(!isFueled);
            }
            else
            {
                arrow.gameObject.SetActive(false);
            }
        }

        // --- HANDLE PUMP UI ARROWS ---
        // Failsafe: If the panel wasn't in the scene at Start, try finding it again
        if (apexPumpPanel == null)
        {
            FindPumpPanel();
        }

        if (isObjective2)
        {
            SetupPumpUI();
            UpdatePumpUIArrows();
        }
        else
        {
            // Ensure UI arrows are hidden if we are NOT on Objective 2
            if (apexPumpPanel != null)
            {
                SetArrowState("Arrow_1", false);
                SetArrowState("Arrow_2", false);
                SetArrowState("Arrow_3", false);
            }
        }
    }

    private void FindPumpPanel()
    {
        // Find the Panel (even if it's currently inactive/hidden in the scene)
        var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (var t in allTransforms)
        {
            if (t.name == "ApexPump Panel")
            {
                apexPumpPanel = t.gameObject;
                break;
            }
        }
    }

    private void SetupPumpUI()
    {
        if (isUIHooked || apexPumpPanel == null) return;

        // Hook Resource Buttons
        Transform resContainer = apexPumpPanel.transform.Find("Pump Panel/Resource Button Container");
        if (resContainer != null)
        {
            foreach (var btn in resContainer.GetComponentsInChildren<Button>(true))
            {
                btn.onClick.AddListener(() => clickedResource = true);
            }
        }

        // Hook Amount Buttons
        Transform amtContainer = apexPumpPanel.transform.Find("Pump Panel/Amount Button Container");
        if (amtContainer != null)
        {
            foreach (var btn in amtContainer.GetComponentsInChildren<Button>(true))
            {
                btn.onClick.AddListener(() => clickedAmount = true);
            }
        }

        // Hook Activate Button
        Transform actBtn = apexPumpPanel.transform.Find("Pump Panel/Activate Button");
        if (actBtn != null)
        {
            var btnComponent = actBtn.GetComponent<Button>();
            if (btnComponent != null)
            {
                btnComponent.onClick.AddListener(() => clickedActivate = true);
            }
        }

        isUIHooked = true;
    }

    private void UpdatePumpUIArrows()
    {
        if (apexPumpPanel == null) return;

        // Arrow 1: Visible until a resource is clicked
        SetArrowState("Arrow_1", !clickedResource);
        // Arrow 2: Visible AFTER resource is clicked, UNTIL amount is clicked
        SetArrowState("Arrow_2", clickedResource && !clickedAmount);
        // Arrow 3: Visible AFTER amount is clicked, UNTIL activate is clicked
        SetArrowState("Arrow_3", clickedAmount && !clickedActivate);
    }

    private void SetArrowState(string arrowName, bool state)
    {
        Transform arrow = apexPumpPanel.transform.Find(arrowName);
        if (arrow != null && arrow.gameObject.activeSelf != state)
        {
            arrow.gameObject.SetActive(state);
        }
    }

    // Finds the Arrowdown child attached to the prefab by checking its name
    private Transform GetArrowChild(Transform parent)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Arrow"))
                return child;
        }
        return null;
    }
}