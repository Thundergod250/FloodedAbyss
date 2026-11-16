using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialController : MonoBehaviour
{
    [Header("Objective Assets")]
    public ObjectivesSO objective1;
    public ObjectivesSO objective2;
    public ObjectivesSO objective3;
    public ObjectivesSO objective6;

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
    [SerializeField] private float panel7AutoDismissSeconds = 10f;
    [SerializeField] private float panel65AutoDismissSeconds = 5f;

    [Header("Panel 6 Settings")]
    [Tooltip("If Objective 6 never reports completion, treat it as done as soon as it becomes the active quest.")]
    [SerializeField] private bool triggerPanel6WhenObjective6Active = true;

    [Header("Specific Tutorial References")]
    [SerializeField] private GameObject resourcesPanel;
    [SerializeField] private GameObject workersPanel;
    [SerializeField] private GameObject fuelPanel;
    [SerializeField] private GameObject tutorialTopPanel;
    [SerializeField] private GameObject waterInfrastructurePanel;
    [SerializeField] private Collider pumpCollider;
    [SerializeField] private Collider waterInfraDebrisCollider;
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

    // Objective 3 — house build → auto-complete
    private enum Obj3State { Inactive, ShowArrows, ShowBuildClick, Complete }
    private Obj3State obj3State = Obj3State.Inactive;
    private GameObject tutorialArrows;
    private int housesAtTutorialStart;

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

    // Objective 6 / water infrastructure tutorial
    private bool objective6Completed;
    private bool waterInfraTutorialCompleted;
    private bool panel65Started;
    private GameObject waterInfraArrow;
    private bool waterInfraTutorialShown;
    private bool infraPanelSeenClosed;
    private bool loggedCompletionMember;

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
        waterInfraArrow = FindFirst("WaterInfraArrow")?.gameObject;

        if (tutorialArrows != null) tutorialArrows.SetActive(false);
        if (waterInfraArrow != null) waterInfraArrow.SetActive(false);

        if (FirstScene != null)
            foreach (var obj in FirstScene)
                if (obj != null) obj.SetActive(false);

        if (resourcesPanel != null) resourcesPanel.SetActive(false);
        if (workersPanel != null) workersPanel.SetActive(false);
        if (pumpCollider != null) pumpCollider.enabled = false;
        if (waterInfraDebrisCollider != null) waterInfraDebrisCollider.enabled = false;
        if (floatingResourceSpawner != null) floatingResourceSpawner.SetActive(false);
        if (GreatFloodTimer != null) GreatFloodTimer.gameObject.SetActive(false);
        if (fuelPanel != null) fuelPanel.gameObject.SetActive(false);
        if (tutorialTopPanel != null) tutorialTopPanel.gameObject.SetActive(false);

        ShowPanel(tutorialPanel_1);

        var rm = GameManager.Instance?.ResourceManager;
        if (rm != null && timberResource != null) lastTimberAmount = rm.GetResourceAmount(timberResource);
        if (rm != null && fiberResource != null) lastFiberAmount = rm.GetResourceAmount(fiberResource);
    }

    private void Update()
    {
        if (ObjectiveManager.Instance == null) return;

        var quest = ObjectiveManager.Instance.CurrentQuest;

        // Quest transitions (also fires when quest becomes null)
        if (quest != previousQuest)
        {
            Debug.Log($"Quest changed: {(previousQuest ? previousQuest.name : "null")} -> {(quest ? quest.name : "null")}");

            ObjectivesSO completedQuest = previousQuest;
            previousQuest = quest;

            if (completedQuest != null)
            {
                if (completedQuest == objective1) OnObjective1Complete();
                else if (completedQuest == objective2) OnObjective2Complete();
                else if (completedQuest == objective6) OnObjective6Complete();
            }
        }

        // Objective 6 completion detection (when it stays as CurrentQuest)
        if (!objective6Completed && objective6 != null && quest == objective6)
        {
            if (IsObjective6Done())
                OnObjective6Complete();
            else if (triggerPanel6WhenObjective6Active)
                OnObjective6Complete();
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

        // Dismiss Panel 5 when Building UI opens
        if (!tutorialPanel5Dismissed && tutorialPanel_5 != null && tutorialPanel_5.activeSelf)
        {
            if (buildingPanel == null) buildingPanel = FindFirst("Building Panel")?.gameObject;
            if (buildingPanel != null && buildingPanel.activeInHierarchy)
            {
                tutorialPanel5Dismissed = true;
                if (tutorialPanel_5 != null) tutorialPanel_5.SetActive(false);
                Debug.Log("Panel 5 dismissed. Building UI opened.");
                TryShowWaterInfraTutorial();
            }
        }

        // Dismiss Panel 6 only after the infra panel was seen closed, then opened
        if (waterInfraTutorialShown && !waterInfraTutorialCompleted && waterInfrastructurePanel != null)
        {
            bool open = waterInfrastructurePanel.activeInHierarchy;
            if (!open) infraPanelSeenClosed = true;
            else if (infraPanelSeenClosed) CompleteWaterInfraTutorial();
        }

        // All objectives done
        if (!allObjectivesHandled && obj3State == Obj3State.Complete)
        {
            allObjectivesHandled = true;
            OnAllObjectivesComplete();
        }
    }

    // -------- Objective 6 completion detection --------

    private static readonly string[] CompletionNames =
    {
        "isCompleted", "IsCompleted", "completed", "Completed",
        "isComplete", "IsComplete", "complete", "Complete", "isDone", "IsDone"
    };

    private bool IsObjective6Done()
    {
        if (TryReadBool(objective6, out bool a, out string nameA))
        {
            LogCompletionMember($"ObjectivesSO.{nameA}");
            return a;
        }

        if (TryReadBool(ObjectiveManager.Instance, out bool b, out string nameB))
        {
            LogCompletionMember($"ObjectiveManager.{nameB}");
            return b;
        }

        return false;
    }

    private bool TryReadBool(object target, out bool value, out string memberName)
    {
        value = false;
        memberName = null;
        if (target == null) return false;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var type = target.GetType();

        foreach (var n in CompletionNames)
        {
            var f = type.GetField(n, flags);
            if (f != null && f.FieldType == typeof(bool))
            {
                value = (bool)f.GetValue(target);
                memberName = n;
                return true;
            }

            var p = type.GetProperty(n, flags);
            if (p != null && p.PropertyType == typeof(bool) && p.CanRead && p.GetIndexParameters().Length == 0)
            {
                value = (bool)p.GetValue(target);
                memberName = n;
                return true;
            }
        }
        return false;
    }

    private void LogCompletionMember(string name)
    {
        if (loggedCompletionMember) return;
        loggedCompletionMember = true;
        Debug.Log($"Objective 6 completion flag found: {name}");
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
                if (tutorialTopPanel != null) tutorialTopPanel.SetActive(true);
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
        if (tutorialPanel_4point5 != null) tutorialPanel_4point5.SetActive(false);
        ShowPanel(tutorialPanel_5);
    }

    private void OnObjective6Complete()
    {
        if (objective6Completed) return;
        objective6Completed = true;

        if (waterInfraDebrisCollider != null)
            waterInfraDebrisCollider.enabled = true;

        TryShowWaterInfraTutorial();
    }

    private void TryShowWaterInfraTutorial()
    {
        Debug.Log(
            $"Panel 6 check | Objective6: {objective6Completed} | " +
            $"Panel5Dismissed: {tutorialPanel5Dismissed} | " +
            $"AlreadyShown: {waterInfraTutorialShown}");

        if (!objective6Completed || !tutorialPanel5Dismissed) return;
        if (waterInfraTutorialShown) return;

        if (tutorialPanel_6 == null)
        {
            Debug.LogError("Panel 6 reference is missing in Inspector!");
            return;
        }

        waterInfraTutorialShown = true;

        if (waterInfraArrow == null)
            waterInfraArrow = FindFirst("WaterInfraArrow")?.gameObject;

        ShowPanel(tutorialPanel_6);

        if (waterInfraArrow != null)
            waterInfraArrow.SetActive(true);

        Debug.Log($"SUCCESS: Panel 6 opened! activeInHierarchy={tutorialPanel_6.activeInHierarchy}");
    }

    private void CompleteWaterInfraTutorial()
    {
        if (waterInfraTutorialCompleted) return;
        waterInfraTutorialCompleted = true;

        if (tutorialPanel_6 != null) tutorialPanel_6.SetActive(false);
        if (waterInfraArrow != null) waterInfraArrow.SetActive(false);

        TryStartPanel65();
    }

    private void TryStartPanel65()
    {
        if (panel65Started ||
            !waterInfraTutorialCompleted ||
            obj3State != Obj3State.Complete)
            return;

        panel65Started = true;
        panel65DismissRoutine = StartCoroutine(ShowPanel65ThenPanel7());
    }

    private void OnAllObjectivesComplete()
    {
        // Hide panels 1-5 (panel 6 is handled by its own flow)
        for (int i = 0; i < 5; i++)
        {
            if (tutorialPanels[i] != null)
                tutorialPanels[i].SetActive(false);
        }

        if (FirstScene != null)
            foreach (var obj in FirstScene)
                if (obj != null) obj.SetActive(true);

        if (pumpCollider != null)
            pumpCollider.enabled = true;
    }

    private IEnumerator ShowPanel65ThenPanel7()
    {
        if (tutorialPanel_6point5 != null) tutorialPanel_6point5.SetActive(true);
        if (fuelPanel != null) fuelPanel.SetActive(true);

        yield return new WaitForSeconds(panel65AutoDismissSeconds);

        if (tutorialPanel_6point5 != null) tutorialPanel_6point5.SetActive(false);
        if (tutorialPanel_7 != null) tutorialPanel_7.SetActive(true);

        panel65DismissRoutine = null;

        if (panel7AutoDismissSeconds > 0f && tutorialPanel_7 != null)
        {
            if (panel7DismissRoutine != null) StopCoroutine(panel7DismissRoutine);
            panel7DismissRoutine = StartCoroutine(AutoDismissPanel7());
        }
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
                    obj3State = Obj3State.Complete;
                    TryStartPanel65();
                }
                break;
        }

        ApplyObj3Visibility();
    }

    private void ApplyObj3Visibility()
    {
        bool showArrows = obj3State == Obj3State.ShowArrows;
        bool showBuildClick = obj3State == Obj3State.ShowBuildClick;

        if (tutorialArrows == null) tutorialArrows = FindFirst("TutorialArrows")?.gameObject;
        if (tutorialArrows != null && tutorialArrows.activeSelf != showArrows)
            tutorialArrows.SetActive(showArrows);

        foreach (var g in FindAll("ClickBuildStructure"))
            if (g.gameObject.activeSelf != showBuildClick)
                g.gameObject.SetActive(showBuildClick);
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

        if (panel == tutorialPanel_3 && FirstScene != null && FirstScene.Length > 0 && FirstScene[0] != null)
            FirstScene[0].SetActive(true);
    }

    private void HideAllPanels()
    {
        foreach (var p in tutorialPanels)
            if (p != null && p.activeSelf) p.SetActive(false);
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