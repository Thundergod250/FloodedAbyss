using UnityEngine;

public abstract class Structures : MonoBehaviour
{
    private Collider structureCollider; // Assign the main collider
    
    protected bool isSubmerged = false;

    protected virtual void Awake()
    {
        if (structureCollider == null)
            structureCollider = GetComponent<Collider>();
    }

    protected virtual void Update()
    {
        CheckSubmersion();
    }

    protected virtual void CheckSubmersion()
    {
        if (GameManager.Instance == null || GameManager.Instance.WaterLevel == null) return;

        float currentWaterHeight = GameManager.Instance.WaterLevel.CurrentWaterHeight;
        float objectWorldY = transform.position.y; // Use World Position Y

        // Check if the water level has risen above this structure's World Y position
        bool shouldBeSubmerged = objectWorldY <= currentWaterHeight;

        // DEBUG: Uncomment this line temporarily to see values in your console
        // Debug.Log($"[Submersion Check] {gameObject.name} World Y: {objectWorldY} vs Water Height: {currentWaterHeight} -> Submerged: {shouldBeSubmerged}");

        if (shouldBeSubmerged != isSubmerged)
        {
            isSubmerged = shouldBeSubmerged;
            OnSubmersionStateChanged(isSubmerged);
        }
    }

    protected virtual void OnSubmersionStateChanged(bool submerged)
    {
        // Disable collider so raycasts/clicks pass right through it
        if (structureCollider != null)
        {
            structureCollider.enabled = !submerged;
        }

        // If it was working or active, you can force-stop it when it gets flooded!
        if (submerged)
        {
            HandleSubmergedShutdown();
        }
    }

    protected virtual void HandleSubmergedShutdown()
    {
        // Override in derived classes (e.g., stop harvesting or shut down pump switches)
    }

    public abstract void Activate();
    public bool IsSubmerged => isSubmerged;
}