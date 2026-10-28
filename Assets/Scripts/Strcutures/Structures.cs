using UnityEngine;

public abstract class Structures : MonoBehaviour
{
    [Header("Submersion Settings")]
    [SerializeField] protected bool ignoreSubmersion = false;

    [Header("Destruction Settings")]
    [Tooltip("The Buildable/Debris prefab spawned when this structure is destroyed.")]
    [SerializeField] protected GameObject BuildablePrefab;

    protected StructureBuildable owningTerminal;
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
        // By default, built structures are destroyed by floods
        Destroyed();
    }

    public void SetOwningTerminal(StructureBuildable terminal)
    {
        owningTerminal = terminal;
    }

    // The centralized destruction function
    public virtual void Destroyed()
    {
        // 1. Re-enable the building terminal
        if (owningTerminal != null)
        {
            owningTerminal.gameObject.SetActive(true);
        }

        // 2. Spawn the Buildable/Debris prefab
        if (BuildablePrefab != null)
        {
            Instantiate(BuildablePrefab, transform.position, transform.rotation);
        }

        // 3. Destroy this structure
        Destroy(gameObject);
    }

    public abstract void Activate();
    public bool IsSubmerged => isSubmerged;
}