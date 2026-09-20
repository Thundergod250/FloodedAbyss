using System;
using System.Collections.Generic;
using UnityEngine;

public class UiBuilding : UiModals
{
    [Header("UI References")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UIStructureCard structureCardPrefab;
    
    public void PopulateMenu(List<StructureDataSO> structures, Action<StructureDataSO> onStructureChosen)
    {
        // 1. Clear previous card instances
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        if (structures == null || structureCardPrefab == null) return;

        // 2. Instantiate a card for each StructureDataSO asset
        foreach (var structure in structures)
        {
            if (structure == null) continue;

            UIStructureCard cardInstance = Instantiate(structureCardPrefab, cardContainer);

            // Format cost requirements
            List<string> formattedCosts = new List<string>();
            if (structure.CostRequirements != null)
            {
                foreach (var req in structure.CostRequirements)
                {
                    formattedCosts.Add($"{req.resourceType} {req.amount}");
                }
            }

            // Local capture for the click callback
            StructureDataSO currentStructure = structure;

            // Setup card UI elements and pass the build action
            cardInstance.SetupCard(
                currentStructure.StructureName,
                currentStructure.Description,
                formattedCosts,
                currentStructure.StructureIcon,
                () => onStructureChosen?.Invoke(currentStructure)
            );
        }
    }
}