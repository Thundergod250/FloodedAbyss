using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIRepair : UiModals
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI repairPrompt;
    [SerializeField] private TextMeshProUGUI resourceRequiredText;
    [SerializeField] private TextMeshProUGUI resourceRequiredNumberText;

    public Button doRepairButton;

    public void ApplyValues(string ResrouceText, string ResourceNumberText)
    {
        resourceRequiredText.text = ResrouceText;
        resourceRequiredNumberText.text = ResourceNumberText;
    }

    public void ApplyValueToButton(UnityAction action)
    {
        doRepairButton.onClick.RemoveAllListeners();
        doRepairButton.onClick.AddListener(action);
    }
}
