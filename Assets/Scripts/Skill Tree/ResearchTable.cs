using UnityEngine;

public class ResearchTable : Item
{
    public override void Activate()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.SkillTree);
        }
    }
}