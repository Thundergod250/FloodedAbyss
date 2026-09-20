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
    [SerializeField] private List<ResourceRequirement> costRequirements = new();

    [Header("Card Target Reference")]
    [SerializeField] private UIStructureCard structureCard;

    public override void Activate()
    {
        // 1. Open the Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);

        // 2. Format struct requirements into readable strings for UI display
        List<string> formattedCosts = new List<string>();
        foreach (var req in costRequirements)
        {
            formattedCosts.Add($"{req.resourceType} {req.amount}");
        }

        // 3. Pass formatted strings and sprite directly to the card setup
        if (structureCard != null)
        {
            structureCard.SetupCard(
                structureName,
                description,
                formattedCosts,
                structureIcon
            );
        }
        else
        {
            Debug.LogWarning($"[ItemBuildable] Target UIStructureCard is missing on {gameObject.name}!");
        }
    }
    
    public List<ResourceRequirement> GetCostRequirements()
    {
        return costRequirements;
    }
}