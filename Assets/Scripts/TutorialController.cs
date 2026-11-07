using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialController : MonoBehaviour
{
    [Header("Objective Assets")]
    public ObjectivesSO objective1;
    public ObjectivesSO objective2;
    public ObjectivesSO objective3;
    public List<GameObject> currentGlowbox;

    private StructureHarvestResource[] allHarvesters;
    private StructureApexPump[] allPumps;

    // Objective 2 Tracking
    private GameObject apexPumpPanel;
    private bool isUIHooked = false;
    private bool clickedResource = false;
    private bool clickedAmount = false;
    private bool clickedActivate = false;

    // Objective 3 Tracking
    private GameObject tutorialArrows;
    private GameObject buildCardGlowBox;
    private StructureWorkersHouse builtHouse;
    private GameObject housePanelGlowBox;

    private bool hasBuiltHouse = false;
    private bool clickedHouseWorld = false;
    private bool clickedRecruitWorker = false;
    private bool hookedHousePanel = false;

    private void Start()
    {
        allHarvesters = FindObjectsByType<StructureHarvestResource>(FindObjectsInactive.Exclude);
        allPumps = FindObjectsByType<StructureApexPump>(FindObjectsInactive.Exclude);

        FindPumpPanel();

        // Failsafe: Find and disable Tutorial Arrows right at the start
        FindTutorialArrows();
        if (tutorialArrows != null) tutorialArrows.SetActive(false);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null)
            return;

        ObjectivesSO currentQuest = ObjectiveManager.Instance.CurrentQuest;

        bool isObjective1 = (currentQuest == objective1);
        bool isObjective2 = (currentQuest == objective2);
        bool isObjective3 = (currentQuest == objective3);

        // --- OBJECTIVE 1 & 2 ---
        HandleHarvestArrows(isObjective1);
        HandlePumpWorldArrows(isObjective2);
        HandlePumpUIArrows(isObjective2);

        // --- OBJECTIVE 3 ---
        if (isObjective3)
        {
            HandleObjective3();
        }
        else
        {
            // Fail-safe: Ensure everything Obj 3 is hidden if we aren't on Obj 3
            if (tutorialArrows != null) tutorialArrows.SetActive(false);
            if (buildCardGlowBox != null) buildCardGlowBox.SetActive(false);
            if (housePanelGlowBox != null) housePanelGlowBox.SetActive(false);
            if (builtHouse != null)
            {
                Transform arrow = GetArrowChild(builtHouse.transform);
                if (arrow != null) arrow.gameObject.SetActive(false);
            }
        }
    }

    private void HandleObjective3()
    {
        // 1. Tutorial Arrows (Active only until a house is built)
        if (tutorialArrows == null) FindTutorialArrows();
        if (tutorialArrows != null) tutorialArrows.SetActive(!hasBuiltHouse);

        // 2. Check if house is built
        if (builtHouse == null) builtHouse = FindAnyObjectByType<StructureWorkersHouse>();
        if (builtHouse != null) hasBuiltHouse = true;

        // 3. TEST Container Glow Box (Active only until house is built)
        if (buildCardGlowBox == null && !hasBuiltHouse)
        {
            // Find it dynamically while the Building UI is open
            var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include);
            foreach (var t in allTransforms)
            {
                if (t.name.Contains("TEST Container"))
                {
                    Transform glow = t.Find("UI Glow Box");
                    if (glow != null) buildCardGlowBox = glow.gameObject;
                    break;
                }
            }
        }
        if (buildCardGlowBox != null) buildCardGlowBox.SetActive(!hasBuiltHouse);

        // 4. Once the house is built
        if (hasBuiltHouse && builtHouse != null)
        {
            // A. WorkerHouseLVL1 Arrow
            Transform houseArrow = GetArrowChild(builtHouse.transform);
            if (houseArrow != null) houseArrow.gameObject.SetActive(!clickedHouseWorld);

            // B. Workers House Panel Glow Box
            var houseUI = GameManager.Instance.uiController.UiWorkersHouse;
            if (houseUI != null && houseUI.gameObject.activeInHierarchy)
            {
                if (houseUI.IsShowingHouse(builtHouse)) clickedHouseWorld = true;

                if (housePanelGlowBox == null)
                {
                    Transform glow = houseUI.transform.Find("UI Glow Box");
                    if (glow != null) housePanelGlowBox = glow.gameObject;
                }

                if (housePanelGlowBox != null)
                {
                    // Hook the Activate Button using EventTrigger to bypass RemoveAllListeners
                    if (!hookedHousePanel)
                    {
                        var buttons = houseUI.GetComponentsInChildren<Button>(true);
                        foreach (var btn in buttons)
                        {
                            if (btn.name == "Activate Button")
                            {
                                var trigger = btn.gameObject.GetComponent<EventTrigger>();
                                if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
                                var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                                entry.callback.AddListener((data) => { clickedRecruitWorker = true; });
                                trigger.triggers.Add(entry);
                                hookedHousePanel = true;
                                break;
                            }
                        }
                    }
                    housePanelGlowBox.SetActive(!clickedRecruitWorker);
                }
            }
        }
    }

    // --- HELPER METHODS ---

    private void FindTutorialArrows()
    {
        if (tutorialArrows != null) return;
        var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (var t in allTransforms)
        {
            if (t.name == "TutorialArrows")
            {
                tutorialArrows = t.gameObject;
                break;
            }
        }
    }

    private void HandleHarvestArrows(bool isObjective1)
    {
        foreach (var harvester in allHarvesters)
        {
            if (harvester == null) continue;
            Transform arrow = GetArrowChild(harvester.transform);
            if (arrow == null) continue;
            if (isObjective1) arrow.gameObject.SetActive(!harvester.IsWorking);
            else arrow.gameObject.SetActive(false);
        }
    }

    private void HandlePumpWorldArrows(bool isObjective2)
    {
        foreach (var pump in allPumps)
        {
            if (pump == null) continue;
            Transform arrow = GetArrowChild(pump.transform);
            if (arrow == null) continue;
            if (isObjective2)
            {
                bool isFueled = pump.CurrentFuel > 0 || pump.IsPumpActive;
                arrow.gameObject.SetActive(!isFueled);
            }
            else arrow.gameObject.SetActive(false);
        }
    }

    private void HandlePumpUIArrows(bool isObjective2)
    {
        if (apexPumpPanel == null) FindPumpPanel();
        if (isObjective2)
        {
            SetupPumpUI();
            if (apexPumpPanel == null) return;
            SetArrowState("Arrow_1", !clickedResource);
            SetArrowState("Arrow_2", clickedResource && !clickedAmount);
            SetArrowState("Arrow_3", clickedAmount && !clickedActivate);
        }
        else if (apexPumpPanel != null)
        {
            SetArrowState("Arrow_1", false);
            SetArrowState("Arrow_2", false);
            SetArrowState("Arrow_3", false);
        }
    }

    private void FindPumpPanel()
    {
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

        Transform resContainer = apexPumpPanel.transform.Find("Pump Panel/Resource Button Container");
        if (resContainer != null)
            foreach (var btn in resContainer.GetComponentsInChildren<Button>(true))
                btn.onClick.AddListener(() => clickedResource = true);

        Transform amtContainer = apexPumpPanel.transform.Find("Pump Panel/Amount Button Container");
        if (amtContainer != null)
            foreach (var btn in amtContainer.GetComponentsInChildren<Button>(true))
                btn.onClick.AddListener(() => clickedAmount = true);

        Transform actBtn = apexPumpPanel.transform.Find("Pump Panel/Activate Button");
        if (actBtn != null)
        {
            var btnComponent = actBtn.GetComponent<Button>();
            if (btnComponent != null) btnComponent.onClick.AddListener(() => clickedActivate = true);
        }
        isUIHooked = true;
    }

    private void SetArrowState(string arrowName, bool state)
    {
        Transform arrow = apexPumpPanel.transform.Find(arrowName);
        if (arrow != null && arrow.gameObject.activeSelf != state) arrow.gameObject.SetActive(state);
    }

    private Transform GetArrowChild(Transform parent)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Arrow")) return child;
        }
        return null;
    }
}