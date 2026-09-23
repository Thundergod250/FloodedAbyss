using UnityEngine;

public class WallRubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private WaterForecast waterForecast;

    [Header("Water")]
    [SerializeField] private float waterLevelAfterDestruction;
    [SerializeField] private float waterLowerDuration = 5f;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Start()
    {
        if (waterForecast == null) 
            waterForecast = GameManager.Instance.uiController.UIHUD.GetHUDElement<WaterForecast>(UIHUD.HuDPanels.WaterForecast);
    }

    private void OnEnable()
    {
        if (health != null)
            health.EvtOnDied.AddListener(HandleDestroyed);
    }

    private void OnDisable()
    {
        if (health != null)
            health.EvtOnDied.RemoveListener(HandleDestroyed);
    }

    private void HandleDestroyed()
    {
        if (waterForecast != null) 
            waterForecast.StartCoroutine(waterForecast.LowerWaterLevel(waterLevelAfterDestruction, waterLowerDuration));
    }
}