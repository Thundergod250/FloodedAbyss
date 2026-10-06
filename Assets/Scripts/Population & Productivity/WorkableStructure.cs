using System.Collections;
using UnityEngine;

public class WorkableStructure : Item
{
    [Header("Building Info")]
    public string buildingName = "Food Structure";

    [Header("Staffing Settings")]
    [SerializeField] protected int maxWorkers = 5;
    [SerializeField] private int assignedWorkers = 0;

    [Header("Production Settings")]
    [SerializeField] private ResourceType outputResourceType = ResourceType.Food;
    [SerializeField] private int baseOutputAmount = 5;
    [SerializeField] private float productionIntervalSeconds = 1f;

    [Header("Building State")]
    [SerializeField] private bool isBroken = false;

    [Header("Repair Settings")]
    [SerializeField] private ResourceType repairCostType = ResourceType.Wood;
    [SerializeField] private int repairCost = 20;

    protected WaterLevel waterLevel;

    public bool IsBroken => isBroken;

    protected bool IsSubmerged => waterLevel != null && waterLevel.CurrentWaterHeight >= this.transform.position.y;


    private Coroutine productionCoroutine;
    public int MaxWorkers => maxWorkers;
    public int AssignedWorkers => assignedWorkers;
    public ResourceType OutputResourceType => outputResourceType;

    public float LocalEfficiency => maxWorkers > 0 ? (float)assignedWorkers / maxWorkers : 0f;
    public float GlobalEfficiency => PopulationManager.Instance != null ? PopulationManager.Instance.GlobalHappiness : 1.0f;
    public float TotalProductivity => LocalEfficiency * GlobalEfficiency;
    public int CalculatedYield => Mathf.FloorToInt(baseOutputAmount * TotalProductivity);

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            waterLevel = GameManager.Instance.waterLevel;
        }

        productionCoroutine = StartCoroutine(ProductionCycleRoutine());

        Debug.Log(
            $"[{buildingName}] WaterLevel: {waterLevel} | " +
            $"Water Height: {waterLevel?.CurrentWaterHeight} | " +
            $"Building Y: {transform.position.y} | " +
            $"Submerged: {IsSubmerged}"
        );
    }

    public override void Activate()
    {
        if (IsSubmerged)
        {
            Debug.Log($"[{buildingName}] Cannot interact. Building is submerged.");
            return;
        }

        if (IsBroken)
        {
            ShowRepairPrompt();
            return;
        }

        StructurePanel panel = FindAnyObjectByType<StructurePanel>(
            FindObjectsInactive.Include
        );

        if (panel != null)
        {
            panel.OpenStructurePanel(this);
        }
        else if (GameManager.Instance != null &&
                 GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(
                UIController.UIState.Building
            );
        }
    }
    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }

    #region Worker-Related
    public bool TryAddWorker()
    {
        if (assignedWorkers >= maxWorkers)
        {
            Debug.LogWarning($"[{buildingName}] Max worker capacity reached!");
            return false;
        }

        if (PopulationManager.Instance != null && !PopulationManager.Instance.TryAssignWorker())
        {
            return false;
        }

        assignedWorkers++;
        Debug.Log($"[{buildingName}] Worker added ({assignedWorkers}/{maxWorkers}).");
        return true;
    }

    public bool TryRemoveWorker()
    {
        if (IsBroken || IsSubmerged)
        {
            Debug.LogWarning(
                $"[{buildingName}] Cannot assign worker. " +
                $"Building is {(IsBroken ? "broken" : "submerged")}."
            );
            return false;
        }

        if (assignedWorkers <= 0) return false;

        assignedWorkers--;
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UnassignWorker();
        }

        Debug.Log($"[{buildingName}] Worker removed ({assignedWorkers}/{maxWorkers}).");
        return true;
    }

    private IEnumerator ProductionCycleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(productionIntervalSeconds);

            if (IsBroken || IsSubmerged)
                continue;

            int yieldAmount = CalculatedYield;

            if (yieldAmount > 0)
            {
                PlayerResources playerResources = GetPlayerResources();

                if (playerResources != null)
                {
                    playerResources.AddResource(outputResourceType, yieldAmount);
                    Debug.Log($"[{buildingName}] Produced +{yieldAmount} {outputResourceType}");
                }
            }
        }
    }
    #endregion

    #region BrokenState
    public void SetBroken(bool broken)
    {
        if (isBroken == broken)
            return;

        isBroken = broken;

        Debug.Log($"[{buildingName}] Broken state: {isBroken}");
    }

    public void BreakBuilding()
    {
        SetBroken(true);
    }

    public void FixBuilding()
    {
        SetBroken(false);
    }

    public virtual void ShowRepairPrompt()
    {
        if (GameManager.Instance == null ||
         GameManager.Instance.uiController == null)
        {
            Debug.LogWarning(
                $"[{buildingName}] Cannot show repair prompt: UIController missing."
            );
            return;
        }

        UIRepair repairUI =
            GameManager.Instance.uiController.GetComponentInChildren<UIRepair>();

        if (repairUI == null)
        {
            Debug.LogWarning(
                $"[{buildingName}] Cannot show repair prompt: UIRepair not found."
            );
            return;
        }

        GameManager.Instance.uiController.OpenModal(
            UIController.UIState.Repair
        );

        repairUI.ApplyValues(
            repairCostType.ToString(),
            repairCost.ToString()
        );

        repairUI.ApplyValueToButton(OnRepairButtonClicked);
    }

    private void OnRepairButtonClicked()
    {
        bool repaired = RepairBuilding();
    }

    public bool RepairBuilding()
    {
        if (!IsBroken)
            return false;

        if (IsSubmerged)
        {
            Debug.Log($"[{buildingName}] Cannot repair while submerged.");
            return false;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.playerController == null)
            return false;

        PlayerResources playerResources =
            GameManager.Instance.playerController.GetComponent<PlayerResources>();

        if (playerResources == null)
            return false;

        bool hasResources = playerResources.SpendResource(
            repairCostType,
            repairCost
        );

        if (!hasResources)
        {
            Debug.Log(
                $"[{buildingName}] Not enough {repairCostType} to repair."
            );

            return false;
        }

        FixBuilding();

        Debug.Log(
            $"[{buildingName}] Repaired for " +
            $"{repairCost} {repairCostType}."
        );

        return true;
    }
    #endregion
}