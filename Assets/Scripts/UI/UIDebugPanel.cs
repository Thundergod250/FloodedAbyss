using TMPro;
using UnityEngine;

public class UIDebugPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resourceNameText;
    [SerializeField] private TextMeshProUGUI resourceAmountText;

    public ResourceType ResourceType { get; private set; }

    public void Setup(ResourceType type, int initialAmount)
    {
        ResourceType = type;
        resourceNameText.text = type.ToString();
        UpdateAmount(initialAmount);
    }

    public void UpdateAmount(int newAmount)
    {
        resourceAmountText.text = newAmount.ToString();
    }
}
