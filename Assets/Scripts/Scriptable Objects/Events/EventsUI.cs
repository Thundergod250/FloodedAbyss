using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventsUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI eventText;
    public Button exitButton;

    private void Start()
    {
        exitButton.onClick.AddListener(GameManager.Instance.GetComponentInChildren<EventManager>().CloseEvent);
    }
}
