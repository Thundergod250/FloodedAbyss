using UnityEngine;

public class StructureTracker : MonoBehaviour
{
    [SerializeField] private string customStructureTitle = "";

    private void Start() => RegisterWithManager();

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(customStructureTitle) && GameManager.Instance != null && GameManager.Instance.BuildingManager != null)
            GameManager.Instance.BuildingManager.UnregisterStructure(customStructureTitle.Trim());
    }

    private void RegisterWithManager()
    {
        if (!string.IsNullOrEmpty(customStructureTitle) && GameManager.Instance != null &&
            GameManager.Instance.BuildingManager != null)
            GameManager.Instance.BuildingManager.RegisterStructure(customStructureTitle.Trim());
        else if (string.IsNullOrEmpty(customStructureTitle)) 
            Debug.LogWarning($"[StructureTracker] Custom Structure Title is not set on {gameObject.name}!", gameObject);
    }
}