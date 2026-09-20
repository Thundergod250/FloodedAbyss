using UnityEngine;
using UnityEngine.UI;

public class UISkillTree : UiModals
{
    [Header("UI Controls")]
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSkillTree);
        }
    }

    public void CloseSkillTree()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }
}