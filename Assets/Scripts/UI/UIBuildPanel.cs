using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildPanel : UiModals
{
    [Header("UI Containers")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private BuildCardUI cardPrefab;
    [SerializeField] private Button closeButton;

    private ItemBuildable currentBuildableBase;
    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    public void OpenPanel(ItemBuildable buildableBase)
    {
        currentBuildableBase = buildableBase;
        PopulateCards();

        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Building);
        }
    }

    public void ClosePanel()
    {
        if (GameManager.Instance != null && GameManager.Instance.uiController != null)
        {
            GameManager.Instance.uiController.CloseAllModals();
        }
    }

    private void ClearCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null) Destroy(card);
        }
        spawnedCards.Clear();
    }

    private void PopulateCards()
    {
        ClearCards();

        if (currentBuildableBase == null) return;

        List<BuildableOption> optionsToDisplay = currentBuildableBase.CustomOptions;

        if (optionsToDisplay == null || optionsToDisplay.Count == 0)
        {
            Debug.LogWarning($"No buildable options assigned to {currentBuildableBase.gameObject.name}!");
            return;
        }

        foreach (BuildableOption option in optionsToDisplay)
        {
            if (cardPrefab != null && cardContainer != null)
            {
                BuildCardUI cardInstance = Instantiate(cardPrefab, cardContainer);
                cardInstance.SetupCard(option, currentBuildableBase, this);
                spawnedCards.Add(cardInstance.gameObject);
            }
        }
    }

    public void TryConstructBuilding(BuildableOption option, ItemBuildable buildableBase)
    {
        PlayerResources playerResources = GetPlayerResources();
        if (playerResources == null)
        {
            Debug.LogWarning("PlayerResources missing!");
            return;
        }

        if (playerResources.SpendResource(option.resourceType, option.resourceCost))
        {
            GameObject spawnedBuilding = null;
            if (option.prefabToSpawn != null && buildableBase != null)
            {
                Transform targetTransform = buildableBase.SpawnLocation;
                spawnedBuilding = Instantiate(option.prefabToSpawn, targetTransform.position, targetTransform.rotation);
            }

            if (buildableBase != null)
            {
                buildableBase.OnBuildingConstructed(option, spawnedBuilding);
            }

            ClosePanel();
        }
        else
        {
            Debug.Log($"Cannot build {option.title}! Required: {option.resourceCost} {option.resourceType}");
        }
    }

    private PlayerResources GetPlayerResources()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            return GameManager.Instance.playerController.GetComponent<PlayerResources>();
        }
        return FindAnyObjectByType<PlayerResources>();
    }
}