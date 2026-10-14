using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifetimeTimer;
    private bool isInitialized = false;

    private StructureHarvestResource harvestResource;

    private void Awake()
    {
        harvestResource = GetComponent<StructureHarvestResource>();
    }

    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifetimeTimer = lifetime;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Move across the water surface safely
        transform.position += moveDirection * (moveSpeed * Time.deltaTime);

        // Handle auto-despawn timer if left uncollected
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            DespawnItem();
        }
    }

    private void DespawnItem()
    {
        if (harvestResource != null)
        {
            // Safely release workers and clean up UI if it was locked in
            harvestResource.ForceCleanupAndDestroy();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}