using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillTreeNode : MonoBehaviour
{
    [Header("Node Data Reference")]
    [SerializeField] private SkillTreeNode nodeData;

    [Header("UI Component References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private GameObject unlockedIndicator;
    [SerializeField] private GameObject lockedOverlay;

    private void OnEnable()
    {
        UpdateNodeUI();
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.EvtOnNodeUnlocked.AddListener(OnNodeUnlocked);
        }
    }

    private void OnDisable()
    {
        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.EvtOnNodeUnlocked.RemoveListener(OnNodeUnlocked);
        }
    }

    public void SetupNode(SkillTreeNode data)
    {
        nodeData = data;
        UpdateNodeUI();
    }

    public void UpdateNodeUI()
    {
        if (nodeData == null) return;

        if (titleText != null) titleText.text = nodeData.nodeTitle;
        if (costText != null) costText.text = nodeData.isUnlocked ? "Unlocked" : $"{nodeData.resourceCost} {nodeData.resourceType}";

        bool isUnlocked = SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsNodeUnlocked(nodeData);
        bool canUnlock = SkillTreeManager.Instance != null && SkillTreeManager.Instance.CanUnlockNode(nodeData);

        if (unlockedIndicator != null) unlockedIndicator.SetActive(isUnlocked);
        if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked && !canUnlock);

        if (unlockButton != null)
        {
            unlockButton.interactable = canUnlock;
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }
    }

    private void OnUnlockClicked()
    {
        if (nodeData != null && SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.TryUnlockNode(nodeData);
        }
    }

    private void OnNodeUnlocked(SkillTreeNode unlockedNode)
    {
        UpdateNodeUI();
    }
}