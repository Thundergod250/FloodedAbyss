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
    [SerializeField] protected BuildableType buildableType = BuildableType.Standard;

    [Header("Buildable Options")]
    [SerializeField] protected List<BuildableOption> buildableOptions = new List<BuildableOption>();

    [Header("Spawn Position (Optional)")]
    [SerializeField] protected Transform spawnLocation;

    public BuildableType Type => buildableType;
    public List<BuildableOption> CustomOptions => buildableOptions;
    public Transform SpawnLocation => spawnLocation != null ? spawnLocation : transform;

    public override void Activate()
    {
        UIBuildPanel buildPanel = FindFirstObjectByType<UIBuildPanel>(FindObjectsInactive.Include);

        if (buildPanel != null)
        {
            buildPanel.OpenPanel(this);
        }
        else if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
        }
    }

    public virtual void OnBuildingConstructed(BuildableOption option, GameObject spawnedInstance)
    {
        Debug.Log($"Constructed {option.title} at {gameObject.name}");
        gameObject.SetActive(false);
    }
}