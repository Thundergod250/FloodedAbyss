using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UiWorkersHouse : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private GameObject resourceRequirementContainer; // Container holding the resource cost pair

    [Header("Destroy Button References")]
    [SerializeField] private Button destroyButton;
    [SerializeField] private TextMeshProUGUI destroyButtonText;

    [Header("Progress Bar References")]
    [SerializeField] private GameObject productionBarContainer;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI progressDescriptionText;

    private StructureWorkersHouse currentHouse;

    protected override void Initialize()
    {
        base.Initialize();

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        if (destroyButton != null)
        {
            destroyButton.onClick.RemoveAllListeners();
            destroyButton.onClick.AddListener(OnDestroyClicked);
        }

        if (productionBarContainer != null)
            productionBarContainer.SetActive(false);
    }

    private void Update()
    {
        if (IsOpen && currentHouse != null)
        {
            bool showBar = currentHouse.IsWorking || currentHouse.IsDestroying;
            if (productionBarContainer != null && productionBarContainer.activeSelf != showBar)
            {
                productionBarContainer.SetActive(showBar);
            }

            if (showBar && fillImage != null)
            {
                fillImage.fillAmount = currentHouse.ProgressPercentage;
            }

            if (progressDescriptionText != null)
            {
                progressDescriptionText.text = currentHouse.ProgressDescription;
            }

            UpdateActionButtonVisuals();
            UpdateDestroyButtonVisuals();
        }
    }

    public void Setup(StructureWorkersHouse house)
    {
        currentHouse = house;
        if (titleText != null) titleText.text = house.StructureTitle;

        UpdateActionButtonVisuals();
        UpdateDestroyButtonVisuals();
    }

    public bool IsShowingHouse(StructureWorkersHouse house)
    {
        return IsOpen && currentHouse == house;
    }

    private void UpdateActionButtonVisuals()
    {
        if (actionButton != null && currentHouse != null)
        {
            var workerMgr = GameManager.Instance.WorkerManager;

            // Check if this specific house is currently housing someone, OR if there are unreserved homeless workers available
            bool isHousingAction = currentHouse.IsReservedForHousing || (workerMgr != null && workerMgr.HomelessWorker > 0);

            // Hide resource requirements whenever in Housing mode (idle or loading)
            if (resourceRequirementContainer != null)
            {
                resourceRequirementContainer.SetActive(!isHousingAction && !currentHouse.IsOccupied);
            }

            if (currentHouse.IsOccupied)
            {
                actionButton.gameObject.SetActive(false); // Can't click if occupied
            }
            else
            {
                actionButton.gameObject.SetActive(true);
                actionButton.interactable = !currentHouse.IsDestroying;

                if (actionButtonText != null)
                {
                    actionButtonText.text = currentHouse.IsWorking 
                        ? (isHousingAction ? "Stop Housing" : "Stop Recruiting") 
                        : (isHousingAction ? "House Worker" : "Recruit Worker");
                }
            }
        }
    }

    private void UpdateDestroyButtonVisuals()
    {
        if (destroyButton != null && currentHouse != null)
        {
            bool isDestroying = currentHouse.IsDestroying;
            if (destroyButtonText != null)
            {
                destroyButtonText.text = isDestroying ? "Cancel" : "Destroy Structure";
            }

            ColorBlock colors = destroyButton.colors;
            Color targetColor = isDestroying ? Color.red : new Color(1f, 0.85f, 0f);
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.pressedColor = targetColor;
            colors.selectedColor = targetColor;
            destroyButton.colors = colors;
        }
    }

    private void OnActionClicked()
    {
        if (currentHouse != null)
        {
            currentHouse.ToggleHouseAction();
        }
    }

    private void OnDestroyClicked()
    {
        if (currentHouse != null)
        {
            currentHouse.ToggleDestroying();
        }
    }

    public void CompleteProgress()
    {
        if (productionBarContainer != null) productionBarContainer.SetActive(false);
    }
}