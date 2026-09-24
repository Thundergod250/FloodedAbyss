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
    [SerializeField] private bool lockDepositedResources = false; // Lock resources once added

    protected PlayerResources playerResources;

    public ResourceRequirement TargetResource => targetResource;
    public bool CanWithdraw => !lockDepositedResources; // Public check for UI

    protected virtual void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerResources = GameManager.Instance.playerController.PlayerResources;
        }
    }

    public override void Activate()
    {
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Structure);

        UIStructures structureModal = GameManager.Instance.uiController.GetModal<UIStructures>(UIController.UIState.Structure);

        if (structureModal != null)
        {
            structureModal.SetupStructure(this);
        }
        else
        {
            Debug.LogError("[ItemStructure] UIStructures modal instance not found on UIState.Structure!");
        }
    }

    public bool DepositResource(int amount)
    {
        if (playerResources == null) return false;

        int spaceRemaining = targetResource.maxCapacity - targetResource.currentAmount;
        if (spaceRemaining <= 0) return false;

        if (playerResources.TryGetResource(targetResource.resourceType, out int playerStock) && playerStock > 0)
        {
            int transferAmount = Mathf.Min(amount, spaceRemaining, playerStock);
            if (transferAmount <= 0) return false;

            if (playerResources.TrySpendResource(targetResource.resourceType, transferAmount))
            {
                targetResource.currentAmount += transferAmount;
                return true;
            }
        }

        return false;
    }

    public bool WithdrawResource(int amount)
    {
        // Block withdrawal if locked
        if (lockDepositedResources)
        {
            Debug.LogWarning("[ItemStructure] Resources in this structure are locked and cannot be withdrawn!");
            return false;
        }

        if (playerResources == null || targetResource.currentAmount <= 0) return false;

        int withdrawAmount = Mathf.Min(amount, targetResource.currentAmount);
        if (withdrawAmount <= 0) return false;

        playerResources.AddResource(targetResource.resourceType, withdrawAmount);
        targetResource.currentAmount -= withdrawAmount;
        return true;
    }
}