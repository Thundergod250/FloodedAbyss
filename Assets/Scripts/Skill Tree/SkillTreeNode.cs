using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillTreeNode", menuName = "Skill Tree/Node")]
public class SkillTreeNode : ScriptableObject
{
    [Header("Node Information")]
    public string nodeID;
    public string nodeTitle;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Prerequisites")]
    [Tooltip("All nodes in this list must be unlocked before this node becomes available.")]
    public List<SkillTreeNode> prerequisiteNodes = new List<SkillTreeNode>();

    [Header("Resource Cost")]
    public ResourceType resourceType = ResourceType.Stone;
    public int resourceCost = 20;

    [Header("Unlock Status Indicator")]
    public bool isUnlocked = false;
    public bool isDefaultUnlocked = false;

    public void ResetNode()
    {
        isUnlocked = isDefaultUnlocked;
    }
}