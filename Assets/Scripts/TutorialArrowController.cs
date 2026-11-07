using UnityEngine;
using UnityEngine.UI;

public class TutorialArrowController : MonoBehaviour
{
    [Header("Objective Assets")]
    public ObjectivesSO objective1;
    public ObjectivesSO objective2;

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

        // --- HANDLE PUMP UI ARROWS (Objective 2) ---
        if (isObjective2)
        {
            SetupPumpUI();
            UpdatePumpUIArrows();
        }
        else
        {
            // Ensure UI arrows are hidden if we are past objective 2
            if (apexPumpPanel != null)
            {
                Transform a1 = apexPumpPanel.transform.Find("Arrow_1");
                Transform a2 = apexPumpPanel.transform.Find("Arrow_2");
                Transform a3 = apexPumpPanel.transform.Find("Arrow_3");

                if (a1 != null) a1.gameObject.SetActive(false);
                if (a2 != null) a2.gameObject.SetActive(false);
                if (a3 != null) a3.gameObject.SetActive(false);
            }
        }
    }

    private void SetupPumpUI()
    {
        if (isUIHooked) return;

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

        if (apexPumpPanel == null) return;

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

        Transform a1 = apexPumpPanel.transform.Find("Arrow_1");
        Transform a2 = apexPumpPanel.transform.Find("Arrow_2");
        Transform a3 = apexPumpPanel.transform.Find("Arrow_3");

        // Arrow 1: Visible until a resource is clicked
        if (a1 != null)
            a1.gameObject.SetActive(!clickedResource);

        // Arrow 2: Visible AFTER resource is clicked, UNTIL amount is clicked
        if (a2 != null)
            a2.gameObject.SetActive(clickedResource && !clickedAmount);

        // Arrow 3: Visible AFTER amount is clicked, UNTIL activate is clicked
        if (a3 != null)
            a3.gameObject.SetActive(clickedAmount && !clickedActivate);
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