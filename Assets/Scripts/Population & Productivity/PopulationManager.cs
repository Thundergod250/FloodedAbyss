using UnityEngine;
using UnityEngine.Events;

public class PopulationManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent EvtOnPopulationChanged;

    [Header("Global Population Stats")]
    [SerializeField] private int totalPopulation = 0;
    [SerializeField] private int idlePopulation = 0;

    public int TotalPopulation => totalPopulation;
    public int IdlePopulation => idlePopulation;

    public void AddIdlePopulation(int amount)
    {
        if (amount <= 0) return;

        totalPopulation += amount;
        idlePopulation += amount;
        EvtOnPopulationChanged?.Invoke();

        Notification.Display($"+{amount} Citizen(s) arrived!", Color.cyan);
    }

    public bool TryAssignIdle(int amount)
    {
        if (idlePopulation >= amount)
        {
            idlePopulation -= amount;
            EvtOnPopulationChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void UnassignToIdle(int amount)
    {
        idlePopulation += amount;
        EvtOnPopulationChanged?.Invoke();
    }
}