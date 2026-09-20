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

    [Header("Cost Requirements Data")]
    [SerializeField] private List<ResourceRequirement> costRequirements = new List<ResourceRequirement>();

    [Header("Card Target Reference")]
    [SerializeField] private UIStructureCard structureCard;

    public override void Activate()
    {
        // 1. Open Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);

        // 2. Format struct requirements into readable strings for display
        List<string> formattedCosts = new List<string>();
        foreach (var req in costRequirements)
        {
            formattedCosts.Add($"{req.resourceType} {req.amount}");
        }

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
        {
            Debug.LogWarning($"[ItemBuildable] Target UIStructureCard is missing on {gameObject.name}!");
        }
    }

    private void OnBuildButtonClicked()
    {
        // Fetch PlayerResources component (adjust reference if stored in GameManager)
        PlayerResources playerResources = FindObjectOfType<PlayerResources>();

        if (playerResources == null)
        {
            Debug.LogError("[ItemBuildable] PlayerResources instance not found!");
            return;
        }

        // 1. Check if the player can afford all requirements
        if (CanAfford(playerResources))
        {
            // 2. Deduct resources
            DeductResources(playerResources);

            // 3. Debug log success
            Debug.Log($"Successfully built {structureName}!");

            // 4. Close all UI modals
            GameManager.Instance.uiController.CloseAllModals();
        }
        else
        {
            Debug.LogWarning($"Cannot build {structureName}: Insufficient resources!");
        }
    }

    private bool CanAfford(PlayerResources playerResources)
    {
        foreach (var req in costRequirements)
        {
            if (playerResources.GetResource(req.resourceType) < req.amount)
            {
                return false;
            }
        }
        return true;
    }

    private void DeductResources(PlayerResources playerResources)
    {
        foreach (var req in costRequirements)
        {
            playerResources.SpendResource(req.resourceType, req.amount);
        }
    }
}