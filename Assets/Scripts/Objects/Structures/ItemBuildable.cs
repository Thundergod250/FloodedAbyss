using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemBuildable : Item
{
    [System.Serializable]
    public struct ResourceRequirement
    {
        public ResourceType resourceType;
        public int amount;
    }

    [Header("Structure Data")]
    [SerializeField] private string structureName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite structureIcon;

    [Header("Spawn Settings")]
    [Tooltip("The actual structure prefab to instantiate when purchased.")]
    [SerializeField] private GameObject structurePrefab;
    [Tooltip("Optional spawn point transform. If null, uses this object's position and rotation.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Cost Requirements Data")]
    [SerializeField] private List<ResourceRequirement> costRequirements = new List<ResourceRequirement>();

    [Header("Card Target Reference")]
    [SerializeField] private UIStructureCard structureCard;

    private PlayerResources playerResources;
    // Stores reference to the active spawned structure instance
    private GameObject spawnedStructureInstance;

    private void Start()
    {
        playerResources = GameManager.Instance.playerController.PlayerResources; 
    }

    public override void Activate()
    {
        // 1. Open Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);

        // 2. Format struct requirements into readable strings for display
        List<string> formattedCosts = new List<string>();
        foreach (var req in costRequirements) 
            formattedCosts.Add($"{req.resourceType} {req.amount}");

        // 3. Setup Card and pass the Build action callback
        if (structureCard != null)
        {
            structureCard.SetupCard(
                structureName,
                description,
                formattedCosts,
                structureIcon,
                OnBuildButtonClicked
            );
        }
        else
            Debug.LogWarning($"[ItemBuildable] Target UIStructureCard is missing on {gameObject.name}!");
    }

    private void OnBuildButtonClicked()
    {
        if (playerResources == null)
        {
            Debug.LogError("[ItemBuildable] PlayerResources instance not found!");
            return;
        }

        // 1. Check if player has required resources
        if (CanAfford(playerResources))
        {
            // 2. Deduct resources
            DeductResources(playerResources);

            // 3. Spawn the purchased structure object
            BuildStructure();

            // 4. Debug log success
            Debug.Log($"Successfully built {structureName}!");

            // 5. Close all UI modals
            GameManager.Instance.uiController.CloseAllModals();

            // 6. Disable this buildable base object (do not destroy)
            gameObject.SetActive(false);
        }
        else
            Debug.LogWarning($"Cannot build {structureName}: Insufficient resources!");
    }

    private void BuildStructure()
    {
        if (structurePrefab == null)
        {
            Debug.LogError($"[ItemBuildable] Structure Prefab is missing on {gameObject.name}!");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;
        spawnedStructureInstance = Pool.Instantiate(structurePrefab, spawnPos, spawnRot);
    }
    
    /// Call this method when selling or destroying the built structure to reclaim the build slot.
    public void ResetBuildableSlot()
    {
        if (spawnedStructureInstance != null)
        {
            Pool.Destroy(spawnedStructureInstance);
            spawnedStructureInstance = null;
        }

        gameObject.SetActive(true);
    }

    private bool CanAfford(PlayerResources playerResources)
    {
        foreach (var req in costRequirements)
        {
            if (playerResources.GetResource(req.resourceType) < req.amount)
                return false;
        }
        return true;
    }

    private void DeductResources(PlayerResources playerResources)
    {
        foreach (var req in costRequirements) 
            playerResources.SpendResource(req.resourceType, req.amount);
    }
}