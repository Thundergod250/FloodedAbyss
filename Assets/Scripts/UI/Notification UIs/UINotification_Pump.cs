using UnityEngine;
using System.Collections;

public class UINotification_Pump : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lowFuelThreshold = 50f;
    [SerializeField] private float checkInterval = 1f; // Editable check interval in seconds
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private string warningMessage = "WARNING: PUMP LOW FUEL";
    [SerializeField] private string warningMessage2 = "WARNING: PUMP NO FUEL REFUEL IMMEDIATELY";

    private StructureApexPump apexPump;
    private UINotification uiNotification; 
    private Coroutine checkRoutine;

    private void Start()
    {
        apexPump = GetComponent<StructureApexPump>();
        uiNotification = GameManager.Instance.uiController.UiHUD.GetUINotification();
        
        if (checkRoutine == null)
            checkRoutine = StartCoroutine(CheckFuelRoutine());
    }

    private void OnDisable()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
        
        ClearNotification();
    }

    private IEnumerator CheckFuelRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);
        yield return null; // Give singletons a frame to initialize

        while (true)
        {
            if (apexPump != null && uiNotification != null)
            {
                bool isZeroFuel = apexPump.CurrentFuel <= 0;
                bool isLowFuel = apexPump.CurrentFuel < lowFuelThreshold;

                if (isZeroFuel)
                    uiNotification.SetNotificationActive(true, warningMessage2, warningColor);
                else if(isLowFuel)
                    uiNotification.SetNotificationActive(true, warningMessage, warningColor);
                else
                    uiNotification.SetNotificationActive(false);
            }

            yield return wait;
        }
    }

    private void ClearNotification()
    {
        if (uiNotification == null) 
            uiNotification = Object.FindAnyObjectByType<UINotification>();

        if (uiNotification != null) 
            uiNotification.SetNotificationActive(false);
    }
}