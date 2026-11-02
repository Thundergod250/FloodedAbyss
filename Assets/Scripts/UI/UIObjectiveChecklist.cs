using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectiveChecklist : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image statusImage;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Checklist")]
    [SerializeField] private Image check;

    public void Setup(string description, bool completed)
    {
        descriptionText.text = description;
        SetCompleted(completed);
    }

    public void SetCompleted(bool completed)
    {
        if (check != null)
        {
            check.gameObject.SetActive(completed);
        }
    }
}
