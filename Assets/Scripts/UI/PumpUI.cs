using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class PumpUI : MonoBehaviour
{
    [Header("Vars")]
    public TextMeshProUGUI resourceTypeText;
    public TextMeshProUGUI pumpNameText;
    public TextMeshProUGUI resourcesToGiveText;
    public Button buttonPlus1;
    public Button buttonPlus10;
    public Button buttonMinus1;
    public Button buttonMinus10;

    [Header("Time left")]
    [SerializeField] private Slider timeLeft;

    public void CloseUI()
    {
        this.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateResources(int resourcesToGive)
    {
        resourcesToGiveText.text = resourcesToGive.ToString();
    }

    public void PopulateText(string resourceText, string nameText, string resourcesAmountText)
    {
        resourceTypeText.text = resourceText;
        pumpNameText.text = nameText;
        resourcesToGiveText.text = resourcesAmountText;
    }

    public void PopulateButtons(UnityAction call1, UnityAction call10, UnityAction remove1, UnityAction remove10)
    {
        Debug.Log("Populating Buttons");
        buttonPlus1.onClick.AddListener(call1);
        buttonPlus10.onClick.AddListener(call10);
        buttonMinus1.onClick.AddListener(remove1);
        buttonMinus10.onClick.AddListener(remove10);
    }
}
