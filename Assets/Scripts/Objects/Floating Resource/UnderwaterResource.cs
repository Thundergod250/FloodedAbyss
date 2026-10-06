using UnityEngine;

public class UnderwaterResource : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float surfaceOffset = 0.2f;   // Height offset above the water mesh
    [SerializeField] private float bobbingSpeed = 2f;      // Gentle wave frequency
    [SerializeField] private float bobbingAmount = 0.05f;  // Gentle wave amplitude

    [Header("Movement Settings")]
    [SerializeField] private bool allowHorizontalDrift = false; // Set true ONLY if you want it to drift across XZ

    private Vector3 moveDirection = Vector3.zero;
    private float moveSpeed = 0f;
    private WaterLevel waterLevel;

    private void Start()
    {
        FindWaterLevelReference();
    }

    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        moveDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        moveSpeed = speed;

        FindWaterLevelReference();

        if (lifetime > 0f)
        {
            Pool.Destroy(gameObject, lifetime);
        }
    }

    private void FindWaterLevelReference()
    {
        // 1. Check GameManager reference
        if (GameManager.Instance != null && GameManager.Instance.waterLevel != null)
        {
            waterLevel = GameManager.Instance.waterLevel;
        }
        // 2. Fallback to scene search if GameManager reference is missing
        else if (waterLevel == null)
        {
            waterLevel = FindAnyObjectByType<WaterLevel>();
        }
    }

    private void Update()
    {
        if (waterLevel == null)
        {
            FindWaterLevelReference();
            return;
        }

        Vector3 currentPos = transform.position;

        // 1. Optional Horizontal Drift (Only runs if explicit speed & direction are given)
        if (allowHorizontalDrift && moveSpeed > 0f && moveDirection != Vector3.zero)
        {
            currentPos += moveDirection * (moveSpeed * Time.deltaTime);
        }

        // 2. Lock Y-Position directly to WaterLevel height
        float waterSurfaceY = waterLevel.CurrentWaterHeight;
        float waveBobbing = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;

        currentPos.y = waterSurfaceY + surfaceOffset + waveBobbing;

        // Apply updated position
        transform.position = currentPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("DebrisCleanup"))
        {
            Pool.Destroy(gameObject);
        }*/
    }
}