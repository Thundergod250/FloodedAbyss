using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UltraPumpUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UltraPump ultraPump;
    [SerializeField] private Button pumpButtonPrefab;
    [SerializeField] private Transform tabContainer;

    [Header("Pump Information")]
    [SerializeField] private TextMeshProUGUI resourceDrainPerSecondText;
    [SerializeField] private TextMeshProUGUI resourceTypeText;
    [SerializeField] private TextMeshProUGUI waterAdjustmentText;

    public void ApplyPumpInfo(string rdps, string rType, string waterLevel)
    {
        resourceDrainPerSecondText.text = rdps;

        resourceTypeText.text = rType;

        waterAdjustmentText.text = waterLevel;
    }

    public void SetUpPumpUI()
    {
        foreach (Transform child in tabContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Pump pump in ultraPump.WaterPump)
        {
            Button newButton = Instantiate(
                pumpButtonPrefab,
                tabContainer
            );

            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = pump.name;

            newButton.onClick.AddListener(() =>
            {
                ApplyPumpInfo(
                    pump.ResourceDrainPerSecond.ToString(),
                    pump.ResourceType.ToString(),
                    pump.WaterAdjustment.ToString()
                );
            });

            if (ultraPump.WaterPump.Count > 0)
            {
                Pump firstPump = ultraPump.WaterPump[0];

                ApplyPumpInfo(
                    firstPump.ResourceDrainPerSecond.ToString(),
                    firstPump.ResourceType.ToString(),
                    firstPump.WaterAdjustment.ToString()
                );
            }
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
