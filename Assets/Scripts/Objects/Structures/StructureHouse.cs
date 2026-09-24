using System.Collections.Generic;
using UnityEngine;

public class StructureHouse : ItemStructure
{
    [Header("Recruitment Settings")]
    [SerializeField] private List<StructureDataSO.ResourceRequirement> recruitmentCost;

    public IReadOnlyList<StructureDataSO.ResourceRequirement> RecruitmentCost => recruitmentCost;

    private PopulationManager PopManager => GameManager.Instance != null ? GameManager.Instance.PopulationManager : null;

    /// <summary>
    /// Buys 1 Citizen using PlayerResources and adds them to the global Idle Population pool.
    /// </summary>
    public bool TryRecruitCitizen()
    {
        if (playerResources == null || PopManager == null) return false;

        if (playerResources.TrySpendResources(recruitmentCost))
        {
            PopManager.AddIdlePopulation(1);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Assigns an Idle Citizen from PopulationManager into this House.
    /// </summary>
    public override bool DepositResource(int amount)
    {
        if (PopManager == null) return false;

        int spaceRemaining = targetResource.maxCapacity - targetResource.currentAmount;
        if (spaceRemaining <= 0) return false;

        int assignAmount = Mathf.Min(amount, spaceRemaining, PopManager.IdlePopulation);
        if (assignAmount <= 0) return false;

        if (PopManager.TryAssignIdle(assignAmount))
        {
            targetResource.currentAmount += assignAmount;
            OnResourceUpdated();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Unassigns a Citizen from this House back to the Idle Population pool.
    /// </summary>
    public override bool WithdrawResource(int amount)
    {
        if (PopManager == null || targetResource.currentAmount <= 0) return false;

        int unassignAmount = Mathf.Min(amount, targetResource.currentAmount);
        if (unassignAmount <= 0) return false;

        targetResource.currentAmount -= unassignAmount;
        PopManager.UnassignToIdle(unassignAmount);
        OnResourceUpdated();
        return true;
    }
}