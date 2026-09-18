using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ResourceType
{
    Stone,
    Wood,
    Gold,
    Food
}

public class PlayerResources : MonoBehaviour
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    [Header("UI Dynamic Layout")]
    [SerializeField] private Transform resourceGridParent; // Assign your Grid Layout container here
    [SerializeField] private UIDebugPanel resourceItemPrefab; // Assign your UI Card Prefab here

    // Track created UI items to update them easily
    private Dictionary<ResourceType, UIDebugPanel> uiItemMap = new Dictionary<ResourceType, UIDebugPanel>();

    [Header("Optional Pop-Up Text")]
    [SerializeField] private TextMeshProUGUI stoneTextAdd;

    private void Awake()
    {
        InitializeResourcesAndUI();
    }

    private void InitializeResourcesAndUI()
    {
        // Clear any existing dummy children in the grid (like your 'Test' objects)
        foreach (Transform child in resourceGridParent)
        {
            Destroy(child.gameObject);
        }

        // Loop through all enum values dynamically
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;

            // Instantiate a UI row/card for this resource
            UIDebugPanel itemInstance = Instantiate(resourceItemPrefab, resourceGridParent);
            itemInstance.Setup(type, 0);

            uiItemMap[type] = itemInstance;
        }

        if (stoneTextAdd != null) stoneTextAdd.text = string.Empty;
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
        Debug.Log($"{type} increased by {amount}. Total: {resources[type]}");
        UpdateResourceText(type);

        if (type == ResourceType.Stone && stoneTextAdd != null)
        {
            stoneTextAdd.text = $"+{amount}";
            StartCoroutine(HideResourceAddText());
        }
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (resources[type] >= amount)
        {
            resources[type] -= amount;
            Debug.Log($"{type} decreased by {amount}. Total: {resources[type]}");
            UpdateResourceText(type);
            return true;
        }

        Debug.LogWarning($"Not enough {type}!");
        return false;
    }

    public int GetResource(ResourceType type) => resources[type];

    public void UpdateResourceText(ResourceType type)
    {
        if (uiItemMap.TryGetValue(type, out UIDebugPanel uiItem))
        {
            uiItem.UpdateAmount(resources[type]);
        }
    }

    private IEnumerator HideResourceAddText()
    {
        yield return new WaitForSeconds(0.5f);
        if (stoneTextAdd != null) stoneTextAdd.text = string.Empty;
    }
}