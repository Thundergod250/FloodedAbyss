using System.Collections.Generic;
using UnityEngine;

public enum BuildableType
{
    Standard,
    CampShop,
    Workshop
}

[System.Serializable]
public class BuildableOption
{
    public string title = "New Structure";
    public Sprite icon;
    [TextArea(2, 4)]
    public string description = "Structure description goes here.";
    public ResourceType resourceType = ResourceType.Stone;
    public int resourceCost = 10;
    public GameObject prefabToSpawn;
}

public class ItemBuildable : Item
{
    [Header("Buildable Category")]
    [SerializeField] private BuildableType buildableType = BuildableType.Standard;

    [Header("Unique / Custom Options")]
    [Tooltip("Options specific to this buildable base. For CampShop or Workshop, assign their build options here.")]
    [SerializeField] private List<BuildableOption> customBuildableOptions = new List<BuildableOption>();

    [Header("Spawn Position (Optional)")]
    [SerializeField] private Transform spawnLocation;

    public BuildableType Type => buildableType;
    public List<BuildableOption> CustomOptions => customBuildableOptions;
    public Transform SpawnLocation => spawnLocation != null ? spawnLocation : transform;

    public override void Activate()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
        }
        else
        {
            UIController ui = FindAnyObjectByType<UIController>();
            if (ui != null)
            {
                GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
            }
        }
    }

    public void OnBuildingConstructed(BuildableOption option, GameObject spawnedInstance)
    {
        Debug.Log($"Constructed {option.title} at {gameObject.name}");
        gameObject.SetActive(false);
    }
}