using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UiBuildingWI : UiModals
{
    [Header("UI References")]
    [SerializeField] private Transform scrollRect;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject structureCardPrefab;
    [SerializeField] private GameObject resourcePairPrefabTemplate;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI laborCostText;

    private StructureProgression progression;
    private StructureBuildableWI currentBuildable;
    private Coroutine warningRoutine;

    protected override void Initialize()
    {
        base.Initialize();
        if (structureCardPrefab != null) structureCardPrefab.SetActive(false);
        if (resourcePairPrefabTemplate != null) resourcePairPrefabTemplate.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen && currentBuildable != null)
        {
            if (currentBuildable.IsSubmerged)
            {
                Debug.Log("[UiBuildingWI] Buildable terminal is submerged! Force closing UI.");
                GameManager.Instance.uiController.CloseAllModals();
            }
        }
    }

    public void Setup(StructureBuildableWI buildable)
    {
        currentBuildable = buildable;
        progression = GameManager.Instance.structureProgression.GetComponent<StructureProgression>();
        if (warningText != null) warningText.gameObject.SetActive(false);

        if (laborCostText != null)
            laborCostText.text = $"Labor Cost: {buildable.LaborCost}";

        PopulateCards();
    }

    private void PopulateCards()
    {
        if (contentContainer == null || structureCardPrefab == null || currentBuildable == null) return;

        foreach (Transform child in contentContainer)
        {
            if (child.gameObject != structureCardPrefab)
                Destroy(child.gameObject);
        }

        foreach (var structureData in currentBuildable.AvailableStructuresToBuild)
        {
            if (structureData == null) continue;

            if (progression != null && !progression.IsUnlocked(structureData))
                continue;

            GameObject cardGO = Instantiate(structureCardPrefab, contentContainer);
            cardGO.SetActive(true);

            PanelStructureCard cardPanel = cardGO.GetComponent<PanelStructureCard>();
            if (cardPanel != null)
                cardPanel.Setup(structureData, resourcePairPrefabTemplate,
                                (data) => OnCardSelected(data, cardPanel));   // <-- capture card
        }

        scrollRect.GetComponent<ScrollRect>().content = contentContainer.GetComponent<RectTransform>();
    }

    private void OnCardSelected(StructureDataSO structureData, PanelStructureCard clickedCard)
    {
        if (currentBuildable == null) return;

        if (progression != null && !progression.IsUnlocked(structureData))
        {
            ShowWarning("This structure is locked!");
            return;
        }

        bool success = currentBuildable.TryBuildStructure(structureData, out string errorMsg);
        if (success)
        {
            if (progression != null)
                progression.NotifyStructureBuilt(structureData);

            if (ObjectiveManager.Instance != null)
                ObjectiveManager.Instance.NotifyStructureBuilt(structureData);

            GameManager.Instance.uiController.CloseAllModals();
        }
        else
        {
            if (clickedCard != null)
            {
                List<string> missingRequirements = new List<string>();

                // Worker check
                var workerMgr = GameManager.Instance.WorkerManager;
                if (workerMgr != null && workerMgr.CurrentWorker < currentBuildable.LaborCost)
                    missingRequirements.Add("Needs Worker");

                // Resource check
                var resourceManager = GameManager.Instance.ResourceManager;
                if (structureData.Costs != null && resourceManager != null)
                {
                    foreach (var cost in structureData.Costs)
                    {
                        if (cost.resourceDataSo == null) continue;
                        int hasAmount = resourceManager.GetResourceAmount(cost.resourceDataSo);
                        if (hasAmount < cost.amount)
                        {
                            string resName = cost.resourceDataSo.ResourceName;
                            missingRequirements.Add($"Needs {cost.amount} {resName}");
                        }
                    }
                }

                string finalMessage = missingRequirements.Count > 0
                    ? string.Join("\n", missingRequirements)
                    : (!string.IsNullOrEmpty(errorMsg) ? errorMsg : "Cannot Build");

                clickedCard.ShowWarningMessage(finalMessage);
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