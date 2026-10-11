using UnityEngine;

public class TutorialHighlight : MonoBehaviour
{
    public GameObject Highlight;

    private UiTutorial uiTutorial;
    private Interactables interactables;

    private void Start()
    {
        Highlight.SetActive(false);
    }

    public void OnPicked()
    {
        uiTutorial = GameManager.Instance.uiController.GetComponentInChildren<UiTutorial>();

        if (uiTutorial != null)
        {
            uiTutorial.PlayerCollected25Wood();
        }
    }
}
