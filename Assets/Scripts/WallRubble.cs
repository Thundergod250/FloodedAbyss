using UnityEngine;

public class WallRubble : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;

    [Header("Water")]
    [SerializeField] private float waterLevelAfterDestruction;

    private WaterLevel waterLevel;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Start()
    {
        waterLevel = GameManager.Instance.waterLevel;
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
        LowerWaterLevel();

        // Disable the rubble instead of destroying it.
        gameObject.SetActive(false);
    }

    private void LowerWaterLevel()
    {
        if (waterLevel == null)
            return;

        Vector3 currentPosition = waterLevel.waterLevelTransform.position;

        waterLevel.waterLevelTransform.position = new Vector3(
            currentPosition.x,
            waterLevelAfterDestruction,
            currentPosition.z
        );
    }
}