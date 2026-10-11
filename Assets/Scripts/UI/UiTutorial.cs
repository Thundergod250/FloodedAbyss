using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UiTutorial : UiModals
{
    [Header("Player Tutorial State")]
    [SerializeField, Range(1, 10)] private int tutorialStep = 1;

    [Header("Tutorial 1 Assets")] // WASD
    [SerializeField] private GameObject WASDTutorialText;
    [HideInInspector] public GameObject WASDBoxCollider;

    [Header("Tutorial 2 Assets")] // piick 10 wood, highlight wood
    [SerializeField] private GameObject PickWoodTutorialText;
    [SerializeField] private List<GameObject> pickableWood = new List<GameObject>();
    [SerializeField] private GameObject woodenDoor;

    [Header("Tutorial 3 Assets")] // place in pump
    [SerializeField] private GameObject PlaceWoodInPumpTutorialText;
    private ApexPump apexPump;

    [Header("Tutorial 4 Assets")] //upgrade pump
    [SerializeField] private GameObject UpgradePumpTutorialText;

    [Header("Tutorial End Assets")] //tutorial End
    [SerializeField] private GameObject TutorialEndText;

    public bool IsTutorialActive;
    private PlayerResources playerResource;

    #region Tutorial Setup
    protected override void Initialize()
    {
        tutorialStep = 1;
        GetAllPickableWood();

        //turn off other tutorial UI
        PickWoodTutorialText.SetActive(false);
        PlaceWoodInPumpTutorialText.SetActive(false);
        UpgradePumpTutorialText.SetActive(false);
        TutorialEndText.SetActive(false);

        apexPump = GameManager.Instance.ApexPump;

        playerResource = GameManager.Instance.playerController.GetComponentInChildren<PlayerResources>();
    }

    private void GetAllPickableWood()
    {
        pickableWood.Clear();

        woodenDoor = GameObject.Find("Broken Door");

        GameObject[] allObjects = FindObjectsByType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "WoodenPlank")
            {
                pickableWood.Add(obj);
            }
        }
    }

    private void TutorialUIState(GameObject UI, bool state)
    {
        UI.SetActive(state);
    }
    #endregion

    #region MovementTutorial 1
    public void EnableMovementTutorial()
    {
        IsTutorialActive = true;

        if (tutorialStep != 1)
            return;

        TutorialUIState(WASDTutorialText, true);

        GameManager.Instance.uiController.OpenModal(UIController.UIState.Tutorial);

        if (WASDBoxCollider != null)
            WASDBoxCollider.SetActive(true);
    }

    public void HideMovementTutorial()
    {
        if (WASDBoxCollider != null)
            WASDBoxCollider.SetActive(false);

        if (tutorialStep == 1)
        {
            TutorialUIState(WASDTutorialText, false);

            tutorialStep = 2;

            HighlightPickableWood();
            TutorialUIState(PickWoodTutorialText, true);
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Tutorial);
        }
    }
    #endregion

    #region CollectWood 2
    public void HighlightPickableWood()
    {
        if (tutorialStep != 2) return; 
        
        foreach (GameObject tutorialWood in pickableWood)
        {
            Debug.Log("Test");
            tutorialWood.GetComponent<TutorialHighlight>().Highlight.SetActive(true);
        }

        woodenDoor.GetComponent<TutorialHighlight>().Highlight.SetActive(true);
    }

    public void PlayerCollected25Wood()
    {
        if (tutorialStep != 2) return; 

        if ((playerResource.GetResource(ResourceType.Wood) >= 24))
        {
            GetAllPickableWood(); // Refresh List remove any null references

            foreach (GameObject tutorialWood in pickableWood)
            {
                tutorialWood.GetComponent<TutorialHighlight>().Highlight.SetActive(false);
            }

            if (woodenDoor != null) //Check if player has not destroyed the wooden door
            {
                woodenDoor.GetComponent<TutorialHighlight>().Highlight.SetActive(false);
            }

            TutorialUIState(PickWoodTutorialText, false);

            tutorialStep = 3;

            TutorialUIState(PlaceWoodInPumpTutorialText, true);
            GameManager.Instance.uiController.OpenModal(UIController.UIState.Tutorial);

            HighlightPump();
        }

    }

    #endregion

    #region PlaceWoodInPump 3
    public void HighlightPump()
    {
        if (tutorialStep != 3) return;

        TutorialHighlight[] highlights = apexPump.GetComponentsInChildren<TutorialHighlight>(true);

        foreach (TutorialHighlight highlight in highlights)
        {
            if (highlight.Highlight != null)
                highlight.Highlight.SetActive(true);
        }

    }

    public void PlayerDeposited10Wood()
    {
        if (tutorialStep != 3) return;

        TutorialUIState(PlaceWoodInPumpTutorialText, false);

        tutorialStep = 4;

        TutorialUIState(UpgradePumpTutorialText, true);

        GameManager.Instance.uiController.OpenModal(UIController.UIState.Tutorial);

        Debug.Log("Tutorial completed: 10 wood deposited into the pump!");
    }
    #endregion

    #region UpgradePump 4
    public void PlayerUpgradedPump()
    {
        if (tutorialStep != 4) return;

        TutorialUIState(UpgradePumpTutorialText, false);


        TutorialHighlight[] highlights = apexPump.GetComponentsInChildren<TutorialHighlight>(true);

        foreach (TutorialHighlight highlight in highlights)
        {
            if (highlight.Highlight != null)
                highlight.Highlight.SetActive(false);
        }

        IsTutorialActive = false;

        TutorialUIState(TutorialEndText, true);
        GameManager.Instance.uiController.OpenModal(UIController.UIState.Tutorial);
    }

    #endregion
}
