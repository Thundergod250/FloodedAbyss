using UnityEngine;
using UnityEngine.Events;

public class PopulationManager : MonoBehaviour
{
    public static PopulationManager Instance { get; private set; }

    [Header("Housing & Population Stats")]
    [SerializeField] private int totalHousingCapacity = 0;
    [SerializeField] private int totalPopulation = 0;
    [SerializeField] private int assignedPopulation = 0;

    [Header("Global Happiness (0.0 to 1.0)")]
    [Range(0f, 1f)]
    [SerializeField] private float globalHappiness = 1.0f; // Default 100% happiness

    [Header("Events")]
    public UnityEvent EvtOnPopulationChanged;

    public int TotalHousingCapacity => totalHousingCapacity;
    public int TotalPopulation => totalPopulation;
    public int AssignedPopulation => assignedPopulation;
    public int AvailablePopulation => Mathf.Max(0, totalPopulation - assignedPopulation);
    public float GlobalHappiness => globalHappiness;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region Housing Management
    public void RegisterHousingCapacity(int capacity)
    {
        totalHousingCapacity += capacity;
        EvtOnPopulationChanged?.Invoke();
        Debug.Log($"[PopulationManager] Housing Capacity increased to {totalHousingCapacity}");
    }

    public void UnregisterHousingCapacity(int capacity)
    {
        totalHousingCapacity = Mathf.Max(0, totalHousingCapacity - capacity);
        EvtOnPopulationChanged?.Invoke();
    }
    #endregion

    #region Employee Hiring
    public bool CanHireEmployee()
    {
        return totalPopulation < totalHousingCapacity;
    }

    public bool TryHireEmployee(ResourceType costType, int costAmount)
    {
        if (!CanHireEmployee())
        {
            Debug.LogWarning("[PopulationManager] Cannot hire: Housing limit reached! Build more Tents.");
            return false;
        }

        PlayerResources playerResources = GetPlayerResources();
        if (playerResources == null) return false;

        if (playerResources.SpendResource(costType, costAmount))
        {
            totalPopulation++;
            EvtOnPopulationChanged?.Invoke();
            Debug.Log($"[PopulationManager] Employee hired! Population: {totalPopulation}/{totalHousingCapacity}");
            return true;
        }

        Debug.LogWarning($"[PopulationManager] Not enough {costType} to hire employee!");
        return false;
    }
    #endregion

    #region Worker Assignment
    public bool TryAssignWorker()
    {
        if (AvailablePopulation <= 0)
        {
            Debug.LogWarning("[PopulationManager] No available unassigned population!");
            return false;
        }

        assignedPopulation++;
        EvtOnPopulationChanged?.Invoke();
        return true;
    }

    public void UnassignWorker()
    {
        if (assignedPopulation > 0)
        {
            assignedPopulation--;
            EvtOnPopulationChanged?.Invoke();
        }
    }
    #endregion

    #region Happiness Control
    public void SetGlobalHappiness(float happinessValue)
    {
        globalHappiness = Mathf.Clamp01(happinessValue);
        EvtOnPopulationChanged?.Invoke();
    }
    #endregion

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }
}