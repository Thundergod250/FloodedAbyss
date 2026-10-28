using UnityEngine;
using TMPro;
using System.Collections;

public class UIOverworld_ApexPump : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private GameObject uiObject; 
    
    [Header("Settings")]
    [SerializeField] private float updateInterval = 2f;

    [Header("Fuel Color Thresholds")]
    [SerializeField] private int lowFuelThreshold = 50;
    [SerializeField] private Color lowFuelColor = Color.red;

    [SerializeField] private int mediumFuelThreshold = 100;
    [SerializeField] private Color mediumFuelColor = new(1f, 0.5f, 0f); // Orange

    [SerializeField] private Color normalFuelColor = Color.white;

    private TextMeshProUGUI powerText;
    private StructureApexPump structureApexPump;
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
                int currentFuel = structureApexPump.CurrentFuel;
                bool hasFuel = currentFuel > 0;

                if (hasFuel)
                {
                    if (uiObject != null && !uiObject.activeSelf) 
                        uiObject.SetActive(true);

                    // Update text string
                    if (powerText != null) 
                    {
                        powerText.text = $"Fuel: {currentFuel}";

                        // Apply color based on thresholds
                        if (currentFuel <= lowFuelThreshold)
                        {
                            powerText.color = lowFuelColor; // Red by default
                        }
                        else if (currentFuel <= mediumFuelThreshold)
                        {
                            powerText.color = mediumFuelColor; // Orange by default
                        }
                        else
                        {
                            powerText.color = normalFuelColor; // White / Normal above 100
                        }
                    }
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