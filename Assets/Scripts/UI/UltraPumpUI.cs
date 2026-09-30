using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UltraPumpUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UltraPump ultraPump;
    [SerializeField] private GameObject pumpUIPrefab;
    [SerializeField] private Transform tabContainer;

    public void SetUpPumpUI()
    {
        foreach (Transform child in tabContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Pump pump in ultraPump.WaterPump)
        {
            GameObject pumpUIObject = Instantiate(pumpUIPrefab, tabContainer);

            PumpUI ui = pumpUIObject.GetComponent<PumpUI>();

            ui.PopulateText(
                pump.ResourceTypeShared.ToString(),
                pump.name,
                pump.ResourceToGive.ToString()
            );

            ui.SetUpTimeSlider(pump);

            ui.SetCovered(pump.BlockedByBarnacle);

            ui.PopulateButtons(
                () =>
                {
                    pump.IncreaseResourceToGive(1);
                    ui.UpdateResources(pump.ResourceToGive);
                },

                () =>
                {
                    pump.IncreaseResourceToGive(10);
                    ui.UpdateResources(pump.ResourceToGive);
                },

                () =>
                {
                    pump.DecreaseResourceToGive(1);
                    ui.UpdateResources(pump.ResourceToGive);
                },

                () =>
                {
                    pump.DecreaseResourceToGive(10);
                    ui.UpdateResources(pump.ResourceToGive);
                }
            );
        }
    }

    public void CloseUI()
    {
        this.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
