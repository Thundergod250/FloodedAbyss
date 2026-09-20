using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildCardUI : MonoBehaviour
{
    /*[Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image pictureImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button buildButton;
    [SerializeField] private TextMeshProUGUI buildButtonText;

    private BuildableOption currentOption;
    private ItemBuildable activeBuildableBase;
    private UIBuildPanel parentPanel;

    public void SetupCard(BuildableOption option, ItemBuildable buildableBase, UIBuildPanel panel)
    {
        currentOption = option;
        activeBuildableBase = buildableBase;
        parentPanel = panel;

        if (titleText != null)
            titleText.text = option.title;

        if (pictureImage != null)
        {
            if (option.icon != null)
            {
                pictureImage.sprite = option.icon;
                pictureImage.gameObject.SetActive(true);
            }
            else
            {
                pictureImage.gameObject.SetActive(false);
            }
        }

        if (descriptionText != null)
            descriptionText.text = option.description;

        if (buildButtonText != null)
            buildButtonText.text = $"{option.resourceCost} {option.resourceType}";

        if (buildButton != null)
        {
            buildButton.onClick.RemoveAllListeners();
            buildButton.onClick.AddListener(OnBuildClicked);
        }
    }

    private void OnBuildClicked()
    {
        if (parentPanel != null)
        {
            parentPanel.TryConstructBuilding(currentOption, activeBuildableBase);
        }
    }*/
}