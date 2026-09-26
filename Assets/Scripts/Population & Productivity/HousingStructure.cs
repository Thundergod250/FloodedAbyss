using UnityEngine;

public class HousingStructure : Item
{
    [Header("Level Control")]
    [Tooltip("Switch between Level 1 (Camp), Level 2 (House), and Level 3 (2-Story House).")]
    [Range(1, 3)]
    [SerializeField] private int currentLevel = 1;

    [Header("Capacity per Level")]
    [SerializeField] private int level1Capacity = 2;
    [SerializeField] private int level2Capacity = 4;
    [SerializeField] private int level3Capacity = 6;

    [Header("Target Components")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private BoxCollider boxCollider;

    [Header("Collider Adjustment")]
    [Tooltip("If checked, the BoxCollider will automatically recalculate its size/center from the active mesh bounds.")]
    [SerializeField] private bool autoFitColliderToBounds = true;

    [Header("Level 1 (Camp) Visuals")]
    [SerializeField] private Mesh level1Mesh;
    [SerializeField] private Material[] level1Materials;
    [SerializeField] private float level1YOffset = 0f;
    [SerializeField] private Vector3 level1ColliderCenter = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 level1ColliderSize = new Vector3(2, 2, 2);

    [Header("Level 2 (House) Visuals")]
    [SerializeField] private Mesh level2Mesh;
    [SerializeField] private Material[] level2Materials;
    [SerializeField] private float level2YOffset = 2f;
    [SerializeField] private Vector3 level2ColliderCenter = new Vector3(0, 2, 0);
    [SerializeField] private Vector3 level2ColliderSize = new Vector3(3, 4, 3);

    [Header("Level 3 (2-Story House) Visuals")]
    [SerializeField] private Mesh level3Mesh;
    [SerializeField] private Material[] level3Materials;
    [SerializeField] private float level3YOffset = 4f;
    [SerializeField] private Vector3 level3ColliderCenter = new Vector3(0, 3, 0);
    [SerializeField] private Vector3 level3ColliderSize = new Vector3(3, 6, 3);

    [Header("Hiring Settings")]
    [SerializeField] private ResourceType hiringCostType = ResourceType.Gold;
    [SerializeField] private int hiringCost = 10;

    private bool isRegistered = false;
    private int registeredCapacity = 0;

    public int CurrentLevel => currentLevel;

    public int HousingCapacity => GetCapacityForLevel(currentLevel);

    private void Awake()
    {
        EnsureColliderReference();
        ApplyVisualsForLevel(currentLevel);
    }

    private void OnValidate()
    {
        currentLevel = Mathf.Clamp(currentLevel, 1, 3);
        EnsureColliderReference();
        ApplyVisualsForLevel(currentLevel);

        if (Application.isPlaying && isRegistered)
        {
            SyncCapacityWithManager();
        }
    }

    private void OnEnable()
    {
        RegisterHousing();
    }

    private void Start()
    {
        RegisterHousing();
    }
    private void OnDisable()
    {
        UnregisterHousing();
    }

    private void EnsureColliderReference()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
    }

    public void SetLevel(int level)
    {
        int targetLevel = Mathf.Clamp(level, 1, 3);
        if (targetLevel == currentLevel && isRegistered) return;

        currentLevel = targetLevel;
        ApplyVisualsForLevel(currentLevel);
        SyncCapacityWithManager();
    }

    public bool Upgrade()
    {
        if (currentLevel < 3)
        {
            SetLevel(currentLevel + 1);
            Debug.Log($"[{gameObject.name}] Upgraded to Level {currentLevel} (Capacity: {HousingCapacity} Workers)");
            return true;
        }

        Debug.LogWarning($"[{gameObject.name}] House is already at max level (Level 3)!");
        return false;
    }

    [ContextMenu("Upgrade House")]
    public void ContextUpgrade() => Upgrade();

    [ContextMenu("Set to Level 1 (Camp)")]
    public void SetToLevel1() => SetLevel(1);

    [ContextMenu("Set to Level 2 (House)")]
    public void SetToLevel2() => SetLevel(2);

    [ContextMenu("Set to Level 3 (2-Story House)")]
    public void SetToLevel3() => SetLevel(3);

    public int GetCapacityForLevel(int level)
    {
        return level switch
        {
            1 => level1Capacity,
            2 => level2Capacity,
            3 => level3Capacity,
            _ => level1Capacity
        };
    }

    private void ApplyVisualsForLevel(int level)
    {
        if (meshFilter == null || meshRenderer == null) return;

        float targetYOffset = 0f;
        Vector3 manualCenter = Vector3.zero;
        Vector3 manualSize = Vector3.one;

        switch (level)
        {
            case 1:
                if (level1Mesh != null) meshFilter.sharedMesh = level1Mesh;
                if (level1Materials != null && level1Materials.Length > 0) meshRenderer.sharedMaterials = level1Materials;
                targetYOffset = level1YOffset;
                manualCenter = level1ColliderCenter;
                manualSize = level1ColliderSize;
                break;
            case 2:
                if (level2Mesh != null) meshFilter.sharedMesh = level2Mesh;
                if (level2Materials != null && level2Materials.Length > 0) meshRenderer.sharedMaterials = level2Materials;
                targetYOffset = level2YOffset;
                manualCenter = level2ColliderCenter;
                manualSize = level2ColliderSize;
                break;
            case 3:
                if (level3Mesh != null) meshFilter.sharedMesh = level3Mesh;
                if (level3Materials != null && level3Materials.Length > 0) meshRenderer.sharedMaterials = level3Materials;
                targetYOffset = level3YOffset;
                manualCenter = level3ColliderCenter;
                manualSize = level3ColliderSize;
                break;
        }

        Vector3 currentPos = meshFilter.transform.localPosition;
        meshFilter.transform.localPosition = new Vector3(currentPos.x, targetYOffset, currentPos.z);

        UpdateColliderBounds(manualCenter, manualSize);
    }

    private void UpdateColliderBounds(Vector3 manualCenter, Vector3 manualSize)
    {
        if (boxCollider == null) return;

        if (autoFitColliderToBounds && meshFilter != null && meshFilter.sharedMesh != null)
        {
            Bounds meshBounds = meshFilter.sharedMesh.bounds;

            Vector3 relativeMeshPos = (meshFilter.transform != transform) ? meshFilter.transform.localPosition : Vector3.zero;

            boxCollider.center = relativeMeshPos + meshBounds.center;
            boxCollider.size = meshBounds.size;
        }
        else
        {
            boxCollider.center = manualCenter;
            boxCollider.size = manualSize;
        }
    }

    private void RegisterHousing()
    {
        if (PopulationManager.Instance == null) return;

        if (!isRegistered)
        {
            isRegistered = true;
            SyncCapacityWithManager();
        }
    }

    private void UnregisterHousing()
    {
        if (isRegistered && PopulationManager.Instance != null && registeredCapacity > 0)
        {
            PopulationManager.Instance.UnregisterHousingCapacity(registeredCapacity);
            registeredCapacity = 0;
            isRegistered = false;
        }
    }

    private void SyncCapacityWithManager()
    {
        if (!isRegistered || PopulationManager.Instance == null) return;

        int targetCapacity = HousingCapacity;
        int delta = targetCapacity - registeredCapacity;

        if (delta > 0)
        {
            PopulationManager.Instance.RegisterHousingCapacity(delta);
            registeredCapacity = targetCapacity;
        }
        else if (delta < 0)
        {
            PopulationManager.Instance.UnregisterHousingCapacity(-delta);
            registeredCapacity = targetCapacity;
        }
    }

    public override void Activate()
    {
        if (PopulationManager.Instance != null)
        {
            PopulationManager.Instance.TryHireEmployee(hiringCostType, hiringCost);
        }
    }
}