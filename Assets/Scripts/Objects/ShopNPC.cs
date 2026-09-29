using UnityEngine;

public class ShopNPC : Item
{
    [Header("Trade Configuration")]
    [SerializeField] private ResourceType depositResource = ResourceType.Wood;
    [SerializeField] private ResourceType getResource = ResourceType.Stone;

    [Header("Exchange Ratio (Per 1 Trade Multiplier)")]
    [Tooltip("How much deposit resource is required per unit multiplier.")]
    [SerializeField] private int depositCostPerUnit = 2;
    [Tooltip("How much output resource is given per unit multiplier.")]
    [SerializeField] private int getYieldPerUnit = 1;

    public ResourceType DepositResource => depositResource;
    public ResourceType GetResource => getResource;
    public int DepositCostPerUnit => depositCostPerUnit;
    public int GetYieldPerUnit => getYieldPerUnit;

    public override void Activate()
    {
        UIShop shopUI = FindAnyObjectByType<UIShop>(FindObjectsInactive.Include);
        if (shopUI != null)
        {
            shopUI.OpenTradeShop(this);
        }
        else if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Shop);
        }
    }
}