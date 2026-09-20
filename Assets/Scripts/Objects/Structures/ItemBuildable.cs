using System.Collections.Generic;
using UnityEngine;

public class ItemBuildable : Item
{
    [Header("Structure Data")]
    [SerializeField] private string structureName;
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite structureIcon;

    [Header("Cost Requirements Data")]
    [Tooltip("List of raw cost strings, e.g. 'Wood 5', 'Stone 5'")]
    [SerializeField] private List<string> costRequirements = new List<string>();

    [Header("Card Target Reference")]
    [SerializeField] private UIStructureCard structureCard;

    public override void Activate()
    {
        // 1. Open the Building Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);

        // 2. Pass source strings and sprite directly to the card setup
        if (structureCard != null)
        {
            structureCard.SetupCard(
                structureName,
                description,
                costRequirements,
                structureIcon
            );
        }
        else
        {
            Debug.LogWarning($"[ItemBuildable] Target UIStructureCard is missing on {gameObject.name}!");
        }
    }
}