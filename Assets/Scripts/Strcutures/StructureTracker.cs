using UnityEngine;

public class StructureTracker : MonoBehaviour
{
    [SerializeField] private StructureDataSO structureDataSO;

    private void Start()
    {
        if (structureDataSO != null && GameManager.Instance != null && GameManager.Instance.BuildingManager != null)
            GameManager.Instance.BuildingManager.RegisterStructure(structureDataSO);
        else if (structureDataSO == null) 
            Debug.LogWarning($"[StructureTracker] StructureDataSO is not assigned on prefab: {gameObject.name}", gameObject);
    }

    private void OnDestroy()
    {
        if (structureDataSO != null && GameManager.Instance != null && GameManager.Instance.BuildingManager != null)
            GameManager.Instance.BuildingManager.UnregisterStructure(structureDataSO);
    }
}