using UnityEngine;

public class PlayerTutorial : MonoBehaviour
{
    private UiTutorial tutorialUI;

    private void Start()
    {
        tutorialUI = GameManager.Instance.uiController.GetComponentInChildren<UiTutorial>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<TutorialObjects>() != null)
        {
            tutorialUI.HideMovementTutorial();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<TutorialObjects>() != null)
        {
            tutorialUI.EnableMovementTutorial();
        }
    }
}
