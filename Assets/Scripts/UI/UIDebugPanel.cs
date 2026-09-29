using TMPro;
using UnityEngine;

public class UIDebugPanel : MonoBehaviour
{
    [Header("Text Columns (Ordered Left to Right)")]
    [SerializeField] private TextMeshProUGUI[] columnTexts;

    public ResourceType ResourceType { get; private set; }
    
    public void SetValues(params string[] values)
    {
        if (columnTexts == null) return;

        for (int i = 0; i < columnTexts.Length; i++)
        {
            if (columnTexts[i] == null) continue;

            if (i < values.Length)
            {
                columnTexts[i].gameObject.SetActive(true);
                columnTexts[i].text = values[i];
            }
            else
            {
                columnTexts[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void UpdateColumn(int columnIndex, string value)
    {
        if (columnTexts != null && columnIndex >= 0 && columnIndex < columnTexts.Length)
        {
            if (columnTexts[columnIndex] != null)
                columnTexts[columnIndex].text = value;
        }
    }
    
    public void SetupResource(ResourceType type, int initialAmount)
    {
        ResourceType = type;
        SetValues(type.ToString(), initialAmount.ToString());
    }

    public void UpdateResourceAmount(int amount) => UpdateColumn(1, amount.ToString());

    public void SetupStat(string itemName, string statType, int level, string value) => SetValues(itemName, statType, $"Lvl {level}", value);

    public void UpdateStat(int level, string value)
    {
        UpdateColumn(2, $"Lvl {level}");
        UpdateColumn(3, value);
    }
}