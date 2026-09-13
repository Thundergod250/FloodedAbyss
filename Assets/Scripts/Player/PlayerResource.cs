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

    [Header("UI Text Resource")]
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI stoneTextAdd;

    private void Awake()
    {
        // Initialize all resources to 0
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }

        stoneText.text = "0";
        stoneTextAdd.text = string.Empty; 
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
        Debug.Log($"{type} increased by {amount}. Total: {resources[type]}");
        UpdateResourceText();

        if (type == ResourceType.Stone)
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
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough {type}!");
            return false;
        }
    }

    public int GetResource(ResourceType type)
    {
        return resources[type];
    }

    public void UpdateResourceText()
    {
        stoneText.text = resources[ResourceType.Stone].ToString();
    }

    private IEnumerator HideResourceAddText()
    {
        yield return new WaitForSeconds(0.5f);

        stoneTextAdd.text = string.Empty;
    }
}