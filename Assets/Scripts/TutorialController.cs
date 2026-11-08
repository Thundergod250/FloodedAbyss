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

    // --- Objective 1 & 2 ---
    private StructureHarvestResource[] allHarvesters;
    private StructureApexPump[] allPumps;
    private GameObject apexPumpPanel;
    private bool pumpUIHooked = false;
    private bool clickedResource, clickedAmount, clickedActivate;

    // --- Objective 3 ---
    private enum Obj3State
    {
        Inactive,
        ShowArrows,        // Tutorial Arrows active (world-space pointers at debris)
        ShowBuildClick,    // ClickBuildStructure glow active
        ShowHouseArrow,    // HouseArrow active (arrow on the built house)
        ShowRecruitClick,  // ClickRecruitWorker glow active (in house panel)
        Complete
    }
    private Obj3State obj3State = Obj3State.Inactive;
    private GameObject tutorialArrows;
    private StructureWorkersHouse builtHouse;
    private int housesAtTutorialStart = 0;
    private bool clickedRecruitWorker = false;
    private bool activateButtonHooked = false;

    // Debug tracking
    private ObjectivesSO previousQuest;

    private void Start()
    {
        allHarvesters = FindObjectsByType<StructureHarvestResource>(FindObjectsInactive.Exclude);
        allPumps = FindObjectsByType<StructureApexPump>(FindObjectsInactive.Exclude);
        FindPumpPanel();

        tutorialArrows = FindFirst("TutorialArrows")?.gameObject;
        if (tutorialArrows != null) tutorialArrows.SetActive(false);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null) return;
        var quest = ObjectiveManager.Instance.CurrentQuest;

        // Debug logs for obj 1 & 2 only (obj 3 completion is logged by state machine)
        if (quest != previousQuest)
        {
            if (previousQuest == objective1) Debug.Log("Objective 1 is completed");
            else if (previousQuest == objective2) Debug.Log("Objective 2 is completed");
            previousQuest = quest;
        }

        HandleHarvestArrows(quest == objective1);
        HandlePumpWorldArrows(quest == objective2);
        HandlePumpUIArrows(quest == objective2);
        HandleObjective3(quest == objective3);
    }

    // ========================================================
    // OBJECTIVE 3 — state machine
    // ========================================================
    private void HandleObjective3(bool isObj3)
    {
        // Start on first frame obj 3 activates
        if (isObj3 && obj3State == Obj3State.Inactive)
        {
            obj3State = Obj3State.ShowArrows;
            housesAtTutorialStart = FindObjectsByType<StructureWorkersHouse>(FindObjectsInactive.Exclude).Length;
            Debug.Log($"[Tutorial] Objective 3 started. Click the debris. (pre-existing houses: {housesAtTutorialStart})");
        }

        switch (obj3State)
        {
            case Obj3State.ShowArrows:
                // Building UI open when structure cards spawn
                if (FindObjectsByType<PanelStructureCard>(FindObjectsInactive.Exclude).Length > 0)
                {
                    obj3State = Obj3State.ShowBuildClick;
                    Debug.Log("[Tutorial] Building UI opened. Click the Worker's House card.");
                }
                break;

            case Obj3State.ShowBuildClick:
                var houses = FindObjectsByType<StructureWorkersHouse>(FindObjectsInactive.Exclude);
                if (houses.Length > housesAtTutorialStart)
                {
                    builtHouse = houses[houses.Length - 1];
                    obj3State = Obj3State.ShowHouseArrow;
                    Debug.Log("[Tutorial] House built. Click the house.");
                }
                break;

            case Obj3State.ShowHouseArrow:
                if (IsHousePanelOpen())
                {
                    obj3State = Obj3State.ShowRecruitClick;
                    Debug.Log("[Tutorial] Panel opened. Click Activate.");
                }
                break;

            case Obj3State.ShowRecruitClick:
                HookActivateButton();
                if (clickedRecruitWorker)
                {
                    obj3State = Obj3State.Complete;
                    Debug.Log("Objective 3 is completed");
                }
                break;
        }

        ApplyObj3Visibility();
    }

    private void ApplyObj3Visibility()
    {
        bool showArrows = obj3State == Obj3State.ShowArrows;
        bool showBuildClick = obj3State == Obj3State.ShowBuildClick;
        bool showHouseArrow = obj3State == Obj3State.ShowHouseArrow;
        bool showRecruitClick = obj3State == Obj3State.ShowRecruitClick;

        // TutorialArrows
        if (tutorialArrows == null) tutorialArrows = FindFirst("TutorialArrows")?.gameObject;
        if (tutorialArrows != null && tutorialArrows.activeSelf != showArrows)
            tutorialArrows.SetActive(showArrows);

        // ClickBuildStructure glow boxes (catch every spawned card)
        foreach (var g in FindAll("ClickBuildStructure"))
            if (g.gameObject.activeSelf != showBuildClick)
                g.gameObject.SetActive(showBuildClick);

        // HouseArrow on the built house
        if (builtHouse != null)
            foreach (var c in builtHouse.GetComponentsInChildren<Transform>(true))
                if (c.name == "HouseArrow")
                {
                    if (c.gameObject.activeSelf != showHouseArrow)
                        c.gameObject.SetActive(showHouseArrow);
                    break;
                }

        // ClickRecruitWorker glow boxes
        foreach (var g in FindAll("ClickRecruitWorker"))
            if (g.gameObject.activeSelf != showRecruitClick)
                g.gameObject.SetActive(showRecruitClick);
    }

    private bool IsHousePanelOpen()
    {
        if (builtHouse == null) return false;
        var houseUI = GameManager.Instance?.uiController?.UiWorkersHouse;
        if (houseUI == null) return false;
        if (houseUI.IsShowingHouse(builtHouse)) return true;

        var modal = houseUI.transform.Find("Workers House");
        return modal != null && modal.gameObject.activeInHierarchy;
    }

    private void HookActivateButton()
    {
        if (activateButtonHooked) return;

        var houseUI = GameManager.Instance?.uiController?.UiWorkersHouse;
        if (houseUI == null) return;

        // Find ONLY the Workers House modal — never touch any other modal's buttons
        Transform modal = null;
        foreach (var t in houseUI.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Workers House") { modal = t; break; }
        }

        if (modal == null)
        {
            Debug.LogWarning("[Tutorial] Could not find Workers House modal to hook Activate Button.");
            return;
        }

        foreach (var btn in modal.GetComponentsInChildren<Button>(true))
        {
            if (btn.name != "Activate Button") continue;

            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ =>
            {
                clickedRecruitWorker = true;
                Debug.Log("[Tutorial] Activate clicked (correct button).");
            });
            trigger.triggers.Add(entry);

            activateButtonHooked = true;
            return;
        }

        Debug.LogWarning("[Tutorial] Activate Button not found in Workers House modal.");
    }

    // ========================================================
    // OBJECTIVES 1 & 2 (unchanged behavior)
    // ========================================================
    private void HandleHarvestArrows(bool active)
    {
        foreach (var h in allHarvesters)
        {
            if (h == null) continue;
            var arrow = GetArrowChild(h.transform);
            if (arrow == null) continue;
            arrow.gameObject.SetActive(active && !h.IsWorking);
        }
    }

    private void HandlePumpWorldArrows(bool active)
    {
        foreach (var p in allPumps)
        {
            if (p == null) continue;
            var arrow = GetArrowChild(p.transform);
            if (arrow == null) continue;
            arrow.gameObject.SetActive(active && !(p.CurrentFuel > 0 || p.IsPumpActive));
        }
    }

    private void HandlePumpUIArrows(bool active)
    {
        if (apexPumpPanel == null) FindPumpPanel();
        if (apexPumpPanel == null) return;

        if (active)
        {
            SetupPumpUI();
            SetArrowState("Arrow_1", !clickedResource);
            SetArrowState("Arrow_2", clickedResource && !clickedAmount);
            SetArrowState("Arrow_3", clickedAmount && !clickedActivate);
        }
        else
        {
            SetArrowState("Arrow_1", false);
            SetArrowState("Arrow_2", false);
            SetArrowState("Arrow_3", false);
        }
    }

    private void SetupPumpUI()
    {
        if (pumpUIHooked) return;

        var resContainer = apexPumpPanel.transform.Find("Pump Panel/Resource Button Container");
        if (resContainer != null)
            foreach (var b in resContainer.GetComponentsInChildren<Button>(true))
                b.onClick.AddListener(() => clickedResource = true);

        var amtContainer = apexPumpPanel.transform.Find("Pump Panel/Amount Button Container");
        if (amtContainer != null)
            foreach (var b in amtContainer.GetComponentsInChildren<Button>(true))
                b.onClick.AddListener(() => clickedAmount = true);

        var actBtn = apexPumpPanel.transform.Find("Pump Panel/Activate Button");
        if (actBtn != null)
        {
            var b = actBtn.GetComponent<Button>();
            if (b != null) b.onClick.AddListener(() => clickedActivate = true);
        }

        pumpUIHooked = true;
    }

    private void SetArrowState(string name, bool state)
    {
        var a = apexPumpPanel.transform.Find(name);
        if (a != null && a.gameObject.activeSelf != state) a.gameObject.SetActive(state);
    }

    // ========================================================
    // HELPERS
    // ========================================================
    private void FindPumpPanel()
    {
        var t = FindFirst("ApexPump Panel");
        if (t != null) apexPumpPanel = t.gameObject;
    }

    private Transform FindFirst(string name)
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            if (t.name == name) return t;
        return null;
    }

    private List<Transform> FindAll(string name)
    {
        var list = new List<Transform>();
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            if (t.name == name) list.Add(t);
        return list;
    }

    private Transform GetArrowChild(Transform parent)
    {
        foreach (var c in parent.GetComponentsInChildren<Transform>(true))
            if (c.name.Contains("Arrow")) return c;
        return null;
    }
}