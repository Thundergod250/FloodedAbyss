using UnityEngine;
using TMPro;
using System.Collections;

public class UIOverworld_ApexPump : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private GameObject uiObject; 
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private StructureApexPump structureApexPump;
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 2f;

    private Coroutine updateRoutine;

    private void Awake()
    {
        // Automatically find references if not assigned in Inspector
        if (structureApexPump == null) 
            structureApexPump = GetComponent<StructureApexPump>();

        if (powerText == null && uiObject != null) 
            powerText = uiObject.GetComponentInChildren<TextMeshProUGUI>();
        
        // Ensure this world UI element is turned off at the start
        if (uiObject != null)
            uiObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (updateRoutine == null) 
            updateRoutine = StartCoroutine(UpdateUIRoutine());
    }

    private void OnDisable()
    {
        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }
    }

    private IEnumerator UpdateUIRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            if (structureApexPump != null)
            {
                // Check if the pump has fuel
                bool hasFuel = structureApexPump.CurrentFuel > 0;

                if (hasFuel)
                {
                    if (uiObject != null && !uiObject.activeSelf) 
                        uiObject.SetActive(true);

                    // Fetch Current Fuel instead of Power
                    int currentFuel = structureApexPump.CurrentFuel;
                    if (powerText != null) 
                        powerText.text = $"Fuel: {currentFuel}";
                }
                else
                {
                    if (uiObject != null && uiObject.activeSelf) 
                        uiObject.SetActive(false);
                }
            }

            yield return wait;
        }
    }
}