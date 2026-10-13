using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class UiBuilding : UiModals
{
    [Header("UI References")]
    [SerializeField] private Transform scrollRect;               // Scroll View Content container for cards
    [SerializeField] private Transform contentContainer;         // Scroll View Content container for cards
    [SerializeField] private GameObject structureCardPrefab;     // Prefab with StructureCardPanel attached
    [SerializeField] private GameObject resourcePairPrefabTemplate; // Reference to the resource pair prefab for costs
    [SerializeField] private TextMeshProUGUI warningText;        // Optional warning message if resources are short

    private StructureBuildable currentBuildable;
    private Coroutine warningRoutine;

    protected override void Initialize()
    {
        base.Initialize();
        if (structureCardPrefab != null) structureCardPrefab.SetActive(false);
        if (resourcePairPrefabTemplate != null) resourcePairPrefabTemplate.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public void Setup(StructureBuildable buildable)
    {
        currentBuildable = buildable;
        if (warningText != null) warningText.gameObject.SetActive(false);
        PopulateCards();
    }

    private void PopulateCards()
    {
        if (contentContainer == null || structureCardPrefab == null || currentBuildable == null) return;

        foreach (Transform child in contentContainer)
        {
            if (child.gameObject != structureCardPrefab)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (var structureData in currentBuildable.AvailableStructuresToBuild)
        {
            if (structureData == null) continue;

            GameObject cardGO = Instantiate(structureCardPrefab, contentContainer);
            cardGO.SetActive(true);

            PanelStructureCard cardPanel = cardGO.GetComponent<PanelStructureCard>();
            if (cardPanel != null)
            {
                cardPanel.Setup(structureData, resourcePairPrefabTemplate, OnCardSelected);
            }
        }

        scrollRect.GetComponent<ScrollRect>().content = contentContainer.GetComponent<RectTransform>();
    }

    private void OnCardSelected(StructureDataSO structureData)
    {
        if (currentBuildable != null)
        {
            bool success = currentBuildable.TryBuildStructure(structureData, out string errorMsg);
            if (success)
            {
                GameManager.Instance.uiController.CloseAllModals();
            }
            else
            {
                ShowWarning(errorMsg);
            }
        }
    }

    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            if (warningRoutine != null) StopCoroutine(warningRoutine);
            warningRoutine = StartCoroutine(FlashWarningRoutine());
        }
    }

    private IEnumerator FlashWarningRoutine()
    {
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        warningText.gameObject.SetActive(false);
    }
}