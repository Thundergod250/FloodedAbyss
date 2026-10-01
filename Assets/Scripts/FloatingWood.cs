using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FloatingWood : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float heightOffset = 0.05f;
    [SerializeField] private float floatSpeed = 5f;

    private WaterSurface targetSurface;
    private WaterSearchParameters searchParams;
    private WaterSearchResult searchResult;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            WaterLevel waterLevel = GameManager.Instance.GetComponent<WaterLevel>();
            if (waterLevel != null && waterLevel.waterLevelTransform != null)
            {
                targetSurface = waterLevel.waterLevelTransform.GetComponent<WaterSurface>();
            }
        }

        if (targetSurface == null)
        {
            targetSurface = FindAnyObjectByType<WaterSurface>();
        }
    }

    private void Update()
    {
        if (targetSurface == null) return;

        searchParams.startPositionWS = transform.position;

        // Correct HDRP WaterSurface query method
        if (targetSurface.ProjectPointOnWaterSurface(searchParams, out searchResult))
        {
            float targetY = searchResult.projectedPositionWS.y + heightOffset;
            Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * floatSpeed);
        }
    }
}