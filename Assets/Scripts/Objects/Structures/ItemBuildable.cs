using System.Collections.Generic;
using UnityEngine;

public class ItemBuildable : Item
{
    [Header("Available Structures")]
    [SerializeField] private List<StructureDataSO> availableStructures = new List<StructureDataSO>();

    [Header("Spawn Settings")]
    [Tooltip("Optional spawn point transform. If null, uses this object's position and rotation.")]
    [SerializeField] private Transform spawnPoint;

    private PlayerResources playerResources;
    private GameObject spawnedStructureInstance;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }
    }

    public override void Activate()
    {
        // 1. Open Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);

        // 2. Reference UiBuilding modal instance
        UiBuilding buildingModal = GameManager.Instance.uiController.GetModal<UiBuilding>(UIController.UIState.Building);

        if (buildingModal != null)
        {
            // 3. Pass ScriptableObject list and build action callback
            buildingModal.PopulateMenu(availableStructures, OnBuildStructureSelected);
        }
        else
        {
            Debug.LogError($"[ItemBuildable] UiBuilding modal instance not found on UIState.Building!");
        }
    }

    private void OnBuildStructureSelected(StructureDataSO selectedStructure)
    {
        if (playerResources == null)
        {
            Debug.LogError("[ItemBuildable] PlayerResources instance not found!");
            return;
        }

        if (CanAfford(playerResources, selectedStructure.CostRequirements))
        {
            DeductResources(playerResources, selectedStructure.CostRequirements);
            BuildStructure(selectedStructure.StructurePrefab);

            Debug.Log($"Successfully built {selectedStructure.StructureName}!");
            GameManager.Instance.uiController.CloseAllModals();

            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Cannot build {selectedStructure.StructureName}: Insufficient resources!");
        }
    }

    private void BuildStructure(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[ItemBuildable] Selected structure prefab is missing!");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        spawnedStructureInstance = Pool.Instantiate(prefabToSpawn, spawnPos, spawnRot);
    }

    public void ResetBuildableSlot()
    {
        if (spawnedStructureInstance != null)
        {
            Pool.Destroy(spawnedStructureInstance);
            spawnedStructureInstance = null;
        }

        gameObject.SetActive(true);
    }

    private bool CanAfford(PlayerResources playerResources, IReadOnlyList<StructureDataSO.ResourceRequirement> requirements)
    {
        if (requirements == null) return true;

        foreach (var req in requirements)
        {
            if (playerResources.GetResource(req.resourceType) < req.amount)
                return false;
        }
        return true;
    }

    private void DeductResources(PlayerResources playerResources, IReadOnlyList<StructureDataSO.ResourceRequirement> requirements)
    {
        if (requirements == null) return;

        foreach (var req in requirements)
        {
            playerResources.SpendResource(req.resourceType, req.amount);
        }
    }
}