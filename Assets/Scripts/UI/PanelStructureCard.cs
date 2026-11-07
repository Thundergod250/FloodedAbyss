using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelStructureCard : MonoBehaviour
{
    [Header("Card References")]
    [SerializeField] private Image structureImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Transform costContainer; // Container holding the ResourcePair prefabs for costs
    [SerializeField] private Button cardButton;

    private StructureDataSO boundData;
    private System.Action<StructureDataSO> onCardClickedCallback;

    public void Setup(StructureDataSO data, GameObject resourcePairPrefabTemplate, System.Action<StructureDataSO> onClickCallback)
    {
        boundData = data;
        onCardClickedCallback = onClickCallback;

        if (structureImage != null && data.StructureIcon != null)
            structureImage.sprite = data.StructureIcon;

        if (titleText != null)
            titleText.text = data.StructureTitle;

        if (infoText != null)
            infoText.text = data.StructureInfo;

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => onCardClickedCallback?.Invoke(boundData));
        }

        BuildCostDisplay(resourcePairPrefabTemplate);
    }

    private void BuildCostDisplay(GameObject resourcePairPrefabTemplate)
    {
        if (costContainer == null || resourcePairPrefabTemplate == null || boundData == null) return;

        foreach (Transform child in costContainer)
        {
            if (child.gameObject != resourcePairPrefabTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (var cost in boundData.Costs)
        {
            if (cost.resourceDataSo == null) continue;

            GameObject costGO = Instantiate(resourcePairPrefabTemplate, costContainer);
            costGO.SetActive(true);

            PanelResourcePair pairPanel = costGO.GetComponent<PanelResourcePair>();
            if (pairPanel != null)
            {
                pairPanel.Setup(cost.resourceDataSo, cost.amount);
            }
        }
    }
}
