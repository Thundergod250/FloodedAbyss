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

    [Header("Tutorial Panels (sequential 1-7)")]
    [SerializeField] private GameObject tutorialPanel_1;
    [SerializeField] private GameObject tutorialPanel_2;
    [SerializeField] private GameObject tutorialPanel_3;
    [SerializeField] private GameObject tutorialPanel_4;
    [SerializeField] private GameObject tutorialPanel_5;
    [SerializeField] private GameObject tutorialPanel_6;
    [SerializeField] private GameObject tutorialPanel_7;

    [Header("Scene Element Lists")]
    [Tooltip("Objects hidden for the entire tutorial, restored on full completion.")]
    [SerializeField] private GameObject[] FirstScene;

    [Tooltip("Objects revealed only when the entire questline finishes.")]
    [SerializeField] private GameObject[] SecondScene;

    [Header("Specific Tutorial References")]
    [SerializeField] private GameObject resourcesPanel;
    [SerializeField] private Collider pumpCollider;
    [SerializeField] private GameObject floatingResourceSpawner;
    [SerializeField] private ResourceDataSO timberResource;
    [SerializeField] private ResourceDataSO fiberResource;

    // --- Objective 1 & 2 ---
    private StructureHarvestResource[] allHarvesters;
    private StructureApexPump[] allPumps;
    private GameObject apexPumpPanel;
    private bool pumpUIHooked = false;
    private bool clickedResource, clickedAmount, clickedActivate;

    // --- Objective 3 state ---
    private enum Obj3State { Inactive, ShowArrows, ShowBuildClick, ShowHouseArrow, ShowRecruitClick, Complete }
    private Obj3State obj3State = Obj3State.Inactive;
    private GameObject tutorialArrows;
    private StructureWorkersHouse builtHouse;
    private int housesAtTutorialStart = 0;
    private bool clickedRecruitWorker = false;
    private bool activateButtonHooked = false;

    // --- Tutorial panel progression ---
    private int resourcesCollectedCount = 0;
    private int lastTimberAmount = 0;
    private int lastFiberAmount = 0;
    private bool tutorialPanel4Dismissed = false;
    private bool tutorialPanel5Dismissed = false;

    // --- Finale ---
    private bool allObjectivesHandled = false;

    private GameObject buildingPanel;
    private ObjectivesSO previousQuest;

    // ========================================================
    // LIFECYCLE
    // ========================================================
    private void Start()
    {
        allHarvesters = FindObjectsByType<StructureHarvestResource>(FindObjectsInactive.Exclude);
        allPumps = FindObjectsByType<StructureApexPump>(FindObjectsInactive.Exclude);
        FindPumpPanel();

        buildingPanel = FindFirst("Building Panel")?.gameObject;

        tutorialArrows = FindFirst("TutorialArrows")?.gameObject;
        if (tutorialArrows != null) tutorialArrows.SetActive(false);

        // Hide tutorial-gated UI + spawner + pump collider
        if (FirstScene != null)
            foreach (var obj in FirstScene)
                if (obj != null) obj.SetActive(false);

        if (resourcesPanel != null) resourcesPanel.SetActive(false);
        if (pumpCollider != null) pumpCollider.enabled = false;
        if (floatingResourceSpawner != null) floatingResourceSpawner.SetActive(false);

        // Panel 1 only
        SetPanelActive(tutorialPanel_1, true);
        SetPanelActive(tutorialPanel_2, false);
        SetPanelActive(tutorialPanel_3, false);
        SetPanelActive(tutorialPanel_4, false);
        SetPanelActive(tutorialPanel_5, false);
        SetPanelActive(tutorialPanel_6, false);
        SetPanelActive(tutorialPanel_7, false);

        var rm = GameManager.Instance?.ResourceManager;
        if (rm != null && timberResource != null) lastTimberAmount = rm.GetResourceAmount(timberResource);
        if (rm != null && fiberResource != null) lastFiberAmount = rm.GetResourceAmount(fiberResource);

        Debug.Log("[Tutorial] Panel 1 shown. Collect resources.");
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null) return;

        // === Finale: fires once when ALL objectives are complete ===
        if (!allObjectivesHandled && ObjectiveManager.Instance.AllObjectiveCompleted)
        {
            allObjectivesHandled = true;
            OnAllObjectivesComplete();
        }

        var quest = ObjectiveManager.Instance.CurrentQuest;

        if (quest != previousQuest)
        {
            if (previousQuest == objective1) { Debug.Log("Objective 1 is completed"); OnObjective1Complete(); }
            else if (previousQuest == objective2) { Debug.Log("Objective 2 is completed"); OnObjective2Complete(); }
            previousQuest = quest;
        }

        if (quest == objective1) TrackResourceCollection();

        HandleHarvestArrows(quest == objective1);
        HandlePumpWorldArrows(quest == objective2);
        HandlePumpUIArrows(quest == objective2);
        HandleObjective3(quest == objective3);

        // Panel 4: dismiss on pump UI open
        if (tutorialPanel_4 != null && tutorialPanel_4.activeSelf && !tutorialPanel4Dismissed
            && apexPumpPanel != null && apexPumpPanel.activeInHierarchy)
        {
            tutorialPanel4Dismissed = true;
            tutorialPanel_4.SetActive(false);
            Debug.Log("[Tutorial] Panel 4 dismissed (pump clicked).");
        }

        // Panel 5: dismiss on Building UI open
        if (tutorialPanel_5 != null && tutorialPanel_5.activeSelf && !tutorialPanel5Dismissed)
        {
            if (buildingPanel == null) buildingPanel = FindFirst("Building Panel")?.gameObject;
            if (buildingPanel != null && buildingPanel.activeInHierarchy)
            {
                tutorialPanel5Dismissed = true;
                tutorialPanel_5.SetActive(false);
                Debug.Log("[Tutorial] Panel 5 dismissed (debris clicked).");
            }
        }
    }

    // ========================================================
    // PANEL PROGRESSION
    // ========================================================
    private void TrackResourceCollection()
    {
        if (resourcesCollectedCount >= 2) return;
        var rm = GameManager.Instance?.ResourceManager;
        if (rm == null) return;

        int curTimber = (timberResource != null) ? rm.GetResourceAmount(timberResource) : 0;
        int curFiber = (fiberResource != null) ? rm.GetResourceAmount(fiberResource) : 0;

        int gains = (curTimber > lastTimberAmount ? 1 : 0) + (curFiber > lastFiberAmount ? 1 : 0);

        for (int i = 0; i < gains && resourcesCollectedCount < 2; i++)
        {
            resourcesCollectedCount++;
            if (resourcesCollectedCount == 1)
            {
                if (resourcesPanel != null) resourcesPanel.SetActive(true);
                SetPanelActive(tutorialPanel_2, true);
                Debug.Log("[Tutorial] Panel 2 shown (first resource collected). Resources panel enabled.");
            }
            else if (resourcesCollectedCount == 2)
            {
                SetPanelActive(tutorialPanel_2, false);
                SetPanelActive(tutorialPanel_3, true);
                Debug.Log("[Tutorial] Panel 3 shown (second resource collected).");
            }
        }

        lastTimberAmount = curTimber;
        lastFiberAmount = curFiber;
    }

    private void OnObjective1Complete()
    {
        SetPanelActive(tutorialPanel_3, false);
        if (pumpCollider != null) pumpCollider.enabled = true;
        // NOTE: spawner deliberately stays off until AllObjectiveCompleted
        SetPanelActive(tutorialPanel_4, true);
        Debug.Log("[Tutorial] Panel 4 shown. Pump collider enabled. Click the pump.");
    }

    private void OnObjective2Complete()
    {
        SetPanelActive(tutorialPanel_5, true);
        Debug.Log("[Tutorial] Panel 5 shown. Click the debris.");
    }

    private void OnAllObjectivesComplete()
    {
        // Restore everything that was hidden for the tutorial
        if (FirstScene != null)
            foreach (var obj in FirstScene)
                if (obj != null) obj.SetActive(true);

        if (SecondScene != null)
            foreach (var obj in SecondScene)
                if (obj != null) obj.SetActive(true);

        if (floatingResourceSpawner != null) floatingResourceSpawner.SetActive(true);
        if (pumpCollider != null) pumpCollider.enabled = true;

        Debug.Log("[Tutorial] All objectives complete. Full UI + spawner re-enabled.");
    }

    // ========================================================
    // OBJECTIVE 3 — state machine
    // ========================================================
    private void HandleObjective3(bool isObj3)
    {
        if (isObj3 && obj3State == Obj3State.Inactive)
        {
            obj3State = Obj3State.ShowArrows;
            housesAtTutorialStart = FindObjectsByType<StructureWorkersHouse>(FindObjectsInactive.Exclude).Length;
            Debug.Log($"[Tutorial] Objective 3 started. (pre-existing houses: {housesAtTutorialStart})");
        }

        switch (obj3State)
        {
            case Obj3State.ShowArrows:
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
                    SetPanelActive(tutorialPanel_6, true);
                    Debug.Log("[Tutorial] House built. Panel 6 shown.");
                }
                break;

            case Obj3State.ShowHouseArrow:
                if (IsHousePanelOpen())
                {
                    SetPanelActive(tutorialPanel_6, false);
                    obj3State = Obj3State.ShowRecruitClick;
                    Debug.Log("[Tutorial] Panel 6 dismissed. Click Activate.");
                }
                break;

            case Obj3State.ShowRecruitClick:
                HookActivateButton();
                if (clickedRecruitWorker)
                {
                    obj3State = Obj3State.Complete;
                    SetPanelActive(tutorialPanel_7, true);
                    Debug.Log("Objective 3 is completed. Panel 7 shown.");
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

        if (tutorialArrows == null) tutorialArrows = FindFirst("TutorialArrows")?.gameObject;
        if (tutorialArrows != null && tutorialArrows.activeSelf != showArrows)
            tutorialArrows.SetActive(showArrows);

        foreach (var g in FindAll("ClickBuildStructure"))
            if (g.gameObject.activeSelf != showBuildClick)
                g.gameObject.SetActive(showBuildClick);

        if (builtHouse != null)
            foreach (var c in builtHouse.GetComponentsInChildren<Transform>(true))
                if (c.name == "HouseArrow")
                {
                    if (c.gameObject.activeSelf != showHouseArrow)
                        c.gameObject.SetActive(showHouseArrow);
                    break;
                }

        foreach (var g in FindAll("ClickRecruitWorker"))
            if (g.gameObject.activeSelf != showRecruitClick)
                g.gameObject.SetActive(showRecruitClick);
    }

    // ========================================================
    // OBJECTIVES 1 & 2 (unchanged)
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

        Transform modal = null;
        foreach (var t in houseUI.GetComponentsInChildren<Transform>(true))
            if (t.name == "Workers House") { modal = t; break; }
        if (modal == null) { Debug.LogWarning("[Tutorial] No Workers House modal."); return; }

        foreach (var btn in modal.GetComponentsInChildren<Button>(true))
        {
            if (btn.name != "Activate Button") continue;

            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => { clickedRecruitWorker = true; Debug.Log("[Tutorial] Activate clicked."); });
            trigger.triggers.Add(entry);

            activateButtonHooked = true;
            return;
        }
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        if (panel.activeSelf != active) panel.SetActive(active);
    }

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