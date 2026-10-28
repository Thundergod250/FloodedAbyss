using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UICloseHud : MonoBehaviour
{
    [SerializeField] private GameObject hudObject; 
    private Button closeButton;

    private void Awake() => closeButton = GetComponent<Button>();

    private void OnEnable()
    {
        if (closeButton != null) 
            closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDisable()
    {
        if (closeButton != null) 
            closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked()
    {
        hudObject.SetActive(false);
        GameManager.Instance.SoundManager.Play("SFXButtonClick");
    }
}