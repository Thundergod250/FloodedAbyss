using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectiveChecklist : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image statusImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Checklist Colours")]
    [SerializeField] private Color uncheckedColor = Color.white;
    [SerializeField] private Color checkedColor = Color.green;

    public void Setup(string description, bool completed)
    {
        descriptionText.text = description;
        SetCompleted(completed);
    }

    public void SetCompleted(bool completed)
    {
        if (statusImage == null) return;

        statusImage.color = completed
            ? checkedColor
            : uncheckedColor;
    }
}
