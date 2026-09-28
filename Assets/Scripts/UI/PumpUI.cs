using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PumpUI : MonoBehaviour
{
    [Header("Vars")]
    public TextMeshProUGUI resourceTypeText;
    public TextMeshProUGUI currentResourcesText;
    public TextMeshProUGUI resourcesToGiveText;

    [Header("Time left")]
    [SerializeField] private Slider timeLeft;

    public void CloseUI()
    {
        this.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
