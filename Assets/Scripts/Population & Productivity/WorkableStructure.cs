using System.Collections;
using UnityEngine;

public class WorkableStructure : Item
{
    [Header("Building Info")]
    public string buildingName = "Food Structure";

    [Header("Staffing Settings")]
    [SerializeField] private int maxWorkers = 5;
    [SerializeField] private int assignedWorkers = 0;

    [Header("Production Settings")]
    [SerializeField] private ResourceType outputResourceType = ResourceType.Food;
    [SerializeField] private int baseOutputAmount = 5;
    [SerializeField] private float productionIntervalSeconds = 1f;

    private Coroutine productionCoroutine;

    public int MaxWorkers => maxWorkers;
    public int AssignedWorkers => assignedWorkers;
    public ResourceType OutputResourceType => outputResourceType;

    public float LocalEfficiency => maxWorkers > 0 ? (float)assignedWorkers / maxWorkers : 0f;
    //public float GlobalEfficiency => PopulationManager.Instance != null ? PopulationManager.Instance.GlobalHappiness : 1.0f;
    //public float TotalProductivity => LocalEfficiency * GlobalEfficiency;
    //public int CalculatedYield => Mathf.FloorToInt(baseOutputAmount * TotalProductivity);

    // Changed from private void Start() to protected virtual void Start()
    protected virtual void Start()
    {
        productionCoroutine = StartCoroutine(ProductionCycleRoutine());
    }

    public override void Activate()
    {
        StructurePanel panel = FindAnyObjectByType<StructurePanel>(FindObjectsInactive.Include);
        if (panel != null)
        {
            panel.OpenStructurePanel(this);
        }
        else if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
        }
    }

    public bool TryAddWorker()
    {
        if (assignedWorkers >= maxWorkers)
        {
            Debug.LogWarning($"[{buildingName}] Max worker capacity reached!");
            return false;
        }

        /*if (PopulationManager.Instance != null && !PopulationManager.Instance.TryAssignWorker())
        {
            return false;
        }*/

        assignedWorkers++;
        Debug.Log($"[{buildingName}] Worker added ({assignedWorkers}/{maxWorkers}).");
        return true;
    }

    public bool TryRemoveWorker()
    {
        if (assignedWorkers <= 0) return false;

        assignedWorkers--;
        /*if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.UnassignWorker();
        }*/

        Debug.Log($"[{buildingName}] Worker removed ({assignedWorkers}/{maxWorkers}).");
        return true;
    }

    private IEnumerator ProductionCycleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(productionIntervalSeconds);

            //int yieldAmount = CalculatedYield;

            /*if (yieldAmount > 0)
            {
                PlayerResources playerResources = GetPlayerResources();
                if (playerResources != null)
                {
                    playerResources.AddResource(outputResourceType, yieldAmount);
                    Debug.Log($"[{buildingName}] Produced +{yieldAmount} {outputResourceType}");
                }
            }*/
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
}