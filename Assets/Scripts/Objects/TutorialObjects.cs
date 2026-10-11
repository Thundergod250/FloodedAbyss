using UnityEngine;

public class TutorialObjects : MonoBehaviour
{
   private UiTutorial uiTutorial;

    private void Start()
    {
        uiTutorial = GameManager.Instance.uiController.GetComponentInChildren<UiTutorial>();

        uiTutorial.WASDBoxCollider = this.gameObject;
    }
}
