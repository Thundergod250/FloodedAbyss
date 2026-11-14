using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

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
    [SerializeField] private GameObject tutorialPanel_4point5;
    [SerializeField] private GameObject tutorialPanel_5;
    [SerializeField] private GameObject tutorialPanel_6;
    [SerializeField] private GameObject tutorialPanel_6point5;
    [SerializeField] private GameObject tutorialPanel_7;
    [SerializeField] private GameObject tutorialPanel_8;

    [Header("Scene Elements")]
    [SerializeField] private GameObject[] FirstScene;

    [Header("Finale")]
    [Tooltip("Seconds before panel 7 auto-hides. 0 = leave visible.")]
    [SerializeField] private float panel7AutoDismissSeconds = 5f;
    [SerializeField] private float panel65AutoDismissSeconds;

    [Header("Specific Tutorial References")]
    [SerializeField] private GameObject resourcesPanel;
    [SerializeField] private GameObject workersPanel;
    [SerializeField] private Collider pumpCollider;
    [SerializeField] private GameObject floatingResourceSpawner;
    [SerializeField] private ResourceDataSO timberResource;
    [SerializeField] private ResourceDataSO fiberResource;
    [SerializeField] private TextMeshProUGUI GreatFloodTimer;

    private GameObject[] tutorialPanels;

    // Objective 1 & 2
    private StructureHarvestResource[] allHarvesters;
    private StructureApexPump[] allPumps;
    private GameObject apexPumpPanel;
    private bool pumpUIHooked;
    private bool clickedResource, clickedAmount, clickedActivate;

    // Objective 3
    private enum Obj3State { Inactive, ShowArrows, ShowBuildClick, ShowHouseArrow, ShowRecruitClick, Complete }
    private Obj3State obj3State = Obj3State.Inactive;
    private GameObject tutorialArrows;
    private StructureWorkersHouse builtHouse;
    private int housesAtTutorialStart;
    private bool clickedRecruitWorker;
    private bool activateButtonHooked;

    // Panel progression
    private int resourcesCollectedCount;
    private int lastTimberAmount;
    private int lastFiberAmount;
    private bool tutorialPanel4Dismissed;
    private bool tutorialPanel5Dismissed;

    // Finale
    private bool allObjectivesHandled;
    private Coroutine panel7DismissRoutine;
    private Coroutine panel65DismissRoutine;

    private GameObject buildingPanel;
    private ObjectivesSO previousQuest;

    private void Awake()
    {
        tutorialPanels = new[] {
            tutorialPanel_1, tutorialPanel_2, tutorialPanel_3, tutorialPanel_4,
            tutorialPanel_5, tutorialPanel_6, tutorialPanel_7
        };
    }

    private void Start()
    {
        allHarvesters = FindObjectsByType<StructureHarvestResource>(FindObjectsInactive.Exclude);
        allPumps = FindObjectsByType<StructureApexPump>(FindObjectsInactive.Exclude);
        apexPumpPanel = FindFirst("ApexPump Panel")?.gameObject;
        buildingPanel = FindFirst("Building Panel")?.gameObject;
        tutorialArrows = FindFirst("TutorialArrows")?.gameObject;

        if (tutorialArrows != null) tutorialArrows.SetActive(false);

        if (FirstScene != null)
            foreach (var obj in FirstScene)
                if (obj != null) obj.SetActive(false);

        if (resourcesPanel != null) resourcesPanel.SetActive(false);
        if (workersPanel != null) workersPanel.SetActive(false);
        if (pumpCollider != null) pumpCollider.enabled = false;
        if (floatingResourceSpawner != null) floatingResourceSpawner.SetActive(false);
        if (GreatFloodTimer != null) GreatFloodTimer.gameObject.SetActive(false);

        ShowPanel(tutorialPanel_1);

        var rm = GameManager.Instance?.ResourceManager;
        if (rm != null && timberResource != null) lastTimberAmount = rm.GetResourceAmount(timberResource);
        if (rm != null && fiberResource != null) lastFiberAmount = rm.GetResourceAmount(fiberResource);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null || ObjectiveManager.Instance.CurrentQuest == null) return;

        var quest = ObjectiveManager.Instance.CurrentQuest;

        if (quest != previousQuest)
        {
            if (previousQuest == objective1) OnObjective1Complete();
            else if (previousQuest == objective2) OnObjective2Complete();
            previousQuest = quest;
        }

        if (quest == objective1) TrackResourceCollection();

        HandleHarvestArrows(quest == objective1);
        HandlePumpWorldArrows(quest == objective2);
        HandlePumpUIArrows(quest == objective2);
        HandleObjective3(quest == objective3);

        // Dismiss panel 4 when pump UI opens
        if (!tutorialPanel4Dismissed && tutorialPanel_4 != null && tutorialPanel_4.activeSelf
            && apexPumpPanel != null && apexPumpPanel.activeInHierarchy)
        {
            tutorialPanel4Dismissed = true;
            tutorialPanel_4.SetActive(false);
            tutorialPanel_4point5.SetActive(true);
        }

        // Dismiss panel 5 when building UI opens
        if (!tutorialPanel5Dismissed && tutorialPanel_5 != null && tutorialPanel_5.activeSelf)
        {
            if (buildingPanel == null) buildingPanel = FindFirst("Building Panel")?.gameObject;
            if (buildingPanel != null && buildingPanel.activeInHierarchy)
            {
                tutorialPanel5Dismissed = true;
                tutorialPanel_5.SetActive(false);
            }
        }

        if (!allObjectivesHandled && obj3State == Obj3State.Complete)
        {
            allObjectivesHandled = true;
            OnAllObjectivesComplete();
        }
    }

    // -------- Panel progression --------

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
                if (workersPanel != null) workersPanel.SetActive(true);
                if (floatingResourceSpawner != null) floatingResourceSpawner.SetActive(true);

                ShowPanel(tutorialPanel_2);
            }
            else
            {
                ShowPanel(tutorialPanel_3);
            }
        }

        lastTimberAmount = curTimber;
        lastFiberAmount = curFiber;
    }

    private void OnObjective1Complete()
    {
        if (pumpCollider != null) pumpCollider.enabled = true;
        ShowPanel(tutorialPanel_4);

    }

    private void OnObjective2Complete()
    {
        tutorialPanel_4point5.SetActive(false);
        ShowPanel(tutorialPanel_5);
    }

    private void OnAllObjectivesComplete()
    {
        // Hide panels 1-6, keep 7 as the completion banner
        for (int i = 0; i < 6; i++)
            if (tutorialPanels[i] != null) tutorialPanels[i].SetActive(false);

        if (FirstScene != null)
            foreach (var obj in FirstScene)
                if (obj != null) obj.SetActive(true);

        if (pumpCollider != null) pumpCollider.enabled = true;

        if (panel7DismissRoutine != null) StopCoroutine(panel7DismissRoutine);
        if (panel7AutoDismissSeconds > 0f && tutorialPanel_7 != null)
            panel7DismissRoutine = StartCoroutine(AutoDismissPanel7());
    }

    private IEnumerator ShowPanel65ThenPanel7()
    {
        if (tutorialPanel_6 != null)
            tutorialPanel_6.SetActive(false);

        if (tutorialPanel_6point5 != null)
            tutorialPanel_6point5.SetActive(true);

        yield return new WaitForSeconds(panel65AutoDismissSeconds);

        if (tutorialPanel_6point5 != null)
            tutorialPanel_6point5.SetActive(false);

        if (tutorialPanel_7 != null)
            tutorialPanel_7.SetActive(true);

        panel65DismissRoutine = null;
    }

    private IEnumerator AutoDismissPanel7()
    {
        yield return new WaitForSeconds(panel7AutoDismissSeconds);
        if (tutorialPanel_7 != null) tutorialPanel_7.SetActive(false);
        if (tutorialPanel_8 != null) tutorialPanel_8.SetActive(true);
        if (GreatFloodTimer != null) GreatFloodTimer.gameObject.SetActive(true);
        panel7DismissRoutine = null;
    }

    public void SkipPanel7()
    {
        if (panel7DismissRoutine != null)
        {
            StopCoroutine(panel7DismissRoutine);
            panel7DismissRoutine = null;
        }

        if (tutorialPanel_7 != null) tutorialPanel_7.SetActive(false);

        if (tutorialPanel_8 != null) tutorialPanel_8.SetActive(true);

        if (GreatFloodTimer != null) GreatFloodTimer.gameObject.SetActive(true);
    }

    // -------- Objective 3 state machine --------

    private void HandleObjective3(bool isObj3)
    {
        if (isObj3 && obj3State == Obj3State.Inactive)
        {
            obj3State = Obj3State.ShowArrows;
            housesAtTutorialStart = FindObjectsByType<StructureWorkersHouse>(FindObjectsInactive.Exclude).Length;
        }

        switch (obj3State)
        {
            case Obj3State.ShowArrows:
                if (FindObjectsByType<PanelStructureCard>(FindObjectsInactive.Exclude).Length > 0)
                    obj3State = Obj3State.ShowBuildClick;
                break;

            case Obj3State.ShowBuildClick:
                var houses = FindObjectsByType<StructureWorkersHouse>(FindObjectsInactive.Exclude);
                if (houses.Length > housesAtTutorialStart)
                {
                    builtHouse = houses[houses.Length - 1];
                    obj3State = Obj3State.ShowHouseArrow;
                    ShowPanel(tutorialPanel_6);
                }
                break;

            case Obj3State.ShowHouseArrow:
                if (IsHousePanelOpen())
                {
                    //if (tutorialPanel_6 != null) tutorialPanel_6.SetActive(false);
                    obj3State = Obj3State.ShowRecruitClick;
                }
                break;

            case Obj3State.ShowRecruitClick:
                HookActivateButton();
                if (clickedRecruitWorker && GameManager.Instance.WorkerManager.CurrentWorker > 1)
                {
                    obj3State = Obj3State.Complete;

                    if (panel65DismissRoutine != null)
                        StopCoroutine(panel65DismissRoutine);

                    panel65DismissRoutine = StartCoroutine(ShowPanel65ThenPanel7());
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

    // -------- Objectives 1 & 2 arrows --------

    private void HandleHarvestArrows(bool active)
    {
        foreach (var h in allHarvesters)
        {
            if (h == null) continue;
            var arrow = GetArrowChild(h.transform);
            if (arrow != null) arrow.gameObject.SetActive(active && !h.IsWorking);
        }
    }

    private void HandlePumpWorldArrows(bool active)
    {
        foreach (var p in allPumps)
        {
            if (p == null) continue;
            var arrow = GetArrowChild(p.transform);
            if (arrow != null) arrow.gameObject.SetActive(active && !(p.CurrentFuel > 0 || p.IsPumpActive));
        }
    }

    private void HandlePumpUIArrows(bool active)
    {
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

    // -------- Helpers --------

    private void ShowPanel(GameObject panel)
    {
        HideAllPanels();
        if (panel != null) panel.SetActive(true);

        if(panel == tutorialPanel_3)
        {
            FirstScene[0].SetActive(true);
        }
    }

    private void HideAllPanels()
    {
        foreach (var p in tutorialPanels)
            if (p != null && p.activeSelf) p.SetActive(false);
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

        Transform modal = null;
        foreach (var t in houseUI.GetComponentsInChildren<Transform>(true))
            if (t.name == "Workers House") { modal = t; break; }
        if (modal == null) return;

        foreach (var btn in modal.GetComponentsInChildren<Button>(true))
        {
            if (btn.name != "Activate Button") continue;

            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => clickedRecruitWorker = true);
            trigger.triggers.Add(entry);

            activateButtonHooked = true;
            return;
        }
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