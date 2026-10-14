using UnityEngine;

public abstract class Structures : MonoBehaviour
{
    [Header("Submersion Settings")]
    [SerializeField] protected bool ignoreSubmersion = false; // Check this true on your floating resource prefabs!

    private Collider structureCollider; 
    protected bool isSubmerged = false;

    protected virtual void Awake()
    {
        if (structureCollider == null)
            structureCollider = GetComponent<Collider>();
    }

    protected virtual void Update()
    {
        if (!ignoreSubmersion)
        {
            CheckSubmersion();
        }
    }

    protected virtual void CheckSubmersion()
    {
        if (GameManager.Instance == null || GameManager.Instance.WaterLevel == null) return;

        float currentWaterHeight = GameManager.Instance.WaterLevel.CurrentWaterHeight;
        float objectWorldY = transform.position.y; 

        bool shouldBeSubmerged = objectWorldY <= currentWaterHeight;

        if (shouldBeSubmerged != isSubmerged)
        {
            isSubmerged = shouldBeSubmerged;
            OnSubmersionStateChanged(isSubmerged);
        }
    }

    protected virtual void OnSubmersionStateChanged(bool submerged)
    {
        if (structureCollider != null)
        {
            structureCollider.enabled = !submerged;
        }

        if (submerged)
        {
            HandleSubmergedShutdown();
        }
    }

    protected virtual void HandleSubmergedShutdown()
    {
        // Override in derived classes
    }

    public abstract void Activate();
    public bool IsSubmerged => isSubmerged;
}