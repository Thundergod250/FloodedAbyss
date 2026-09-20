using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance { get; private set; }

    [Header("Registered Tree Nodes")]
    [SerializeField] private List<SkillTreeNode> allNodes = new List<SkillTreeNode>();

    public UnityEvent<SkillTreeNode> EvtOnNodeUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeNodes();
    }

    private void InitializeNodes()
    {
        foreach (var node in allNodes)
        {
            if (node != null)
            {
                node.ResetNode();
            }
        }
    }

    public bool IsNodeUnlocked(SkillTreeNode node)
    {
        return node != null && node.isUnlocked;
    }

    public bool CanUnlockNode(SkillTreeNode node)
    {
        if (node == null || node.isUnlocked) return false;

        foreach (var prereq in node.prerequisiteNodes)
        {
            if (prereq != null && !prereq.isUnlocked)
            {
                return false;
            }
        }

        return true;
    }

    public bool TryUnlockNode(SkillTreeNode node)
    {
        if (!CanUnlockNode(node))
        {
            Debug.LogWarning($"Cannot unlock {node.nodeTitle}: Prerequisites not met or already unlocked!");
            return false;
        }

        PlayerResources playerResources = GetPlayerResources();
        if (playerResources == null) return false;

        if (playerResources.SpendResource(node.resourceType, node.resourceCost))
        {
            node.isUnlocked = true;
            Debug.Log($"Skill Tree Node Unlocked: {node.nodeTitle}!");
            EvtOnNodeUnlocked?.Invoke(node);
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough {node.resourceType} to unlock {node.nodeTitle}!");
            return false;
        }
    }

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }
}