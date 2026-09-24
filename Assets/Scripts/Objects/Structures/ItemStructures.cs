using UnityEngine;

public class ItemStructure : Item
{
    [System.Serializable]
    public struct ResourceRequirement
    {
        public ResourceType resourceType;
        public int currentAmount;
        public int maxCapacity;
    }

    [Header("Structure Storage Settings")]
    [SerializeField] private ResourceRequirement targetResource;

    protected PlayerResources playerResources;

    public ResourceRequirement TargetResource => targetResource;

    protected virtual void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }
    }

    /// <summary>
    /// Triggered when the player interacts with this placed structure.
    /// Opens the Structure UI Modal and passes necessary structure references.
    /// </summary>
    public override void Activate()
    {
        // 1. Open Structure Modal via UIController
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Structure);

        // 2. Fetch UIStructures modal instance
        UIStructures structureModal = GameManager.Instance.uiController.GetModal<UIStructures>(UIController.UIState.Structure);

        if (structureModal != null)
        {
            // 3. Bind structure reference to UI
            structureModal.SetupStructure(this);
        }
        else
        {
            Debug.LogError("[ItemStructure] UIStructures modal instance not found on UIState.Structure!");
        }
    }

    /// <summary>
    /// Transfers resources from player to this structure.
    /// </summary>
    public bool DepositResource(int amount)
    {
        if (playerResources == null) return false;

        // Check remaining capacity in this structure
        int spaceRemaining = targetResource.maxCapacity - targetResource.currentAmount;
        if (spaceRemaining <= 0) return false;

        // Check if player actually owns this resource
        if (playerResources.TryGetResource(targetResource.resourceType, out int playerStock) && playerStock > 0)
        {
            // Calculate how much can actually be transferred
            int transferAmount = Mathf.Min(amount, spaceRemaining, playerStock);
            if (transferAmount <= 0) return false;

            // Spend from player and add to structure
            if (playerResources.TrySpendResource(targetResource.resourceType, transferAmount))
            {
                targetResource.currentAmount += transferAmount;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Takes resources out of this structure and returns them to the player.
    /// </summary>
    public bool WithdrawResource(int amount)
    {
        if (playerResources == null || targetResource.currentAmount <= 0) return false;

        // Cap withdrawal by stored amount
        int withdrawAmount = Mathf.Min(amount, targetResource.currentAmount);
        if (withdrawAmount <= 0) return false;

        // Add back to player and remove from structure
        playerResources.AddResource(targetResource.resourceType, withdrawAmount);
        targetResource.currentAmount -= withdrawAmount;
        return true;
    }
}