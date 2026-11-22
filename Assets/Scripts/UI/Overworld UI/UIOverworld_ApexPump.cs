using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIOverworld_ApexPump : MonoBehaviour
{
    [Serializable]
    public class FuelResourceSlot
    {
        [Tooltip("Assign the ResourceDataSO for this fuel resource.")]
        public ResourceDataSO resourceData;

        [Tooltip("Assign the Pump Resource Pair Container GameObject (or UIOverworld_ResourcePair) for this slot.")]
        public GameObject containerObject;

        [HideInInspector] public Image iconImage;
        [HideInInspector] public TextMeshProUGUI amountText;
        [HideInInspector] public ResourceTooltipTrigger tooltipTrigger;

        public void CacheComponents()
        {
            if (containerObject == null) return;

            var pair = containerObject.GetComponent<UIOverworld_ResourcePair>();
            if (pair != null)
            {
                iconImage = pair.resourceImage;
                amountText = pair.resourceText;
                tooltipTrigger = pair.resourceTooltipTrigger;
            }

            if (iconImage == null)
            {
                Transform iconTransform = containerObject.transform.Find("Resource Image");
                if (iconTransform != null)
                    iconImage = iconTransform.GetComponent<Image>();
            }

            if (amountText == null)
            {
                Transform textTransform = containerObject.transform.Find("Resource Amount Txt");
                if (textTransform != null)
                    amountText = textTransform.GetComponentInChildren<TextMeshProUGUI>();
                else
                    amountText = containerObject.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (tooltipTrigger == null)
            {
                tooltipTrigger = containerObject.GetComponent<ResourceTooltipTrigger>();
                if (tooltipTrigger == null)
                    tooltipTrigger = containerObject.GetComponentInChildren<ResourceTooltipTrigger>();
            }
        }
    }

    [Header("References")]
    [SerializeField] private GameObject uiObject; // Pump Overworld UI (Fuel/Power UI)
    [SerializeField] private GameObject availableResourcesUI; // Pump Available Resource UI
    [SerializeField] private List<FuelResourceSlot> fuelResourceSlots = new List<FuelResourceSlot>(5);

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
    private bool isHovering = false;

    private void Awake()
    {
        if (structureApexPump == null)
            structureApexPump = GetComponent<StructureApexPump>();

        if (powerText == null && uiObject != null)
            powerText = uiObject.GetComponentInChildren<TextMeshProUGUI>();

        foreach (var slot in fuelResourceSlots) 
            slot.CacheComponents();

        if (uiObject != null)
            uiObject.SetActive(false);

        if (availableResourcesUI != null)
            availableResourcesUI.SetActive(false);
    }

    private void OnEnable()
    {
        isHovering = false;
        if (availableResourcesUI != null)
            availableResourcesUI.SetActive(false);

        if (updateRoutine == null)
            updateRoutine = StartCoroutine(UpdateUIRoutine());
    }

    private void OnDisable()
    {
        isHovering = false;

        if (updateRoutine != null)
        {
            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }

        if (availableResourcesUI != null)
            availableResourcesUI.SetActive(false);
    }

    private void OnMouseEnter()
    {
        isHovering = true;
        ShowAvailableResourcesUI();
    }

    private void OnMouseExit()
    {
        isHovering = false;
        HideAvailableResourcesUI();
    }

    public void ShowAvailableResourcesUI()
    {
        if (uiObject != null && uiObject.activeSelf)
            uiObject.SetActive(false);

        if (availableResourcesUI != null && !availableResourcesUI.activeSelf)
            availableResourcesUI.SetActive(true);

        RefreshResourceSlots();
    }

    public void HideAvailableResourcesUI()
    {
        if (availableResourcesUI != null && availableResourcesUI.activeSelf)
            availableResourcesUI.SetActive(false);

        RefreshFuelUI();
    }

    private void RefreshResourceSlots()
    {
        var resourceMgr = GameManager.Instance != null ? GameManager.Instance.ResourceManager : null;

        for (int i = 0; i < fuelResourceSlots.Count; i++)
        {
            FuelResourceSlot slot = fuelResourceSlots[i];
            if (slot == null || slot.containerObject == null) continue;

            if (slot.iconImage == null || slot.amountText == null || slot.tooltipTrigger == null)
                slot.CacheComponents();

            bool hasResource = slot.resourceData != null;
            slot.containerObject.SetActive(hasResource);
            if (!hasResource) continue;

            if (slot.iconImage != null && slot.resourceData.Icon != null)
                slot.iconImage.sprite = slot.resourceData.Icon;

            int amount = 0;
            if (resourceMgr != null)
            {
                // Matches your project's ResourceManager lookup if available; falls back safely to 0
                amount = GetPlayerResourceAmount(resourceMgr, slot.resourceData);
            }

            if (slot.amountText != null)
                slot.amountText.text = amount.ToString();

            if (slot.tooltipTrigger != null)
                slot.tooltipTrigger.SetResource(slot.resourceData);
        }
    }

    private int GetPlayerResourceAmount(object resourceMgr, ResourceDataSO data)
    {
        // Adjust this call if your ResourceManager uses a specific method name like GetResourceCount(data)
        var type = resourceMgr.GetType();
        var method = type.GetMethod("GetResourceAmount")
                  ?? type.GetMethod("GetResourceCount")
                  ?? type.GetMethod("GetAmount");

        if (method != null)
        {
            object result = method.Invoke(resourceMgr, new object[] { data });
            if (result is int intVal) return intVal;
        }

        return 0;
    }

    private void RefreshFuelUI()
    {
        if (structureApexPump == null) return;

        int currentFuel = structureApexPump.CurrentFuel;
        bool hasFuel = currentFuel > 0;

        if (hasFuel)
        {
            if (uiObject != null && !uiObject.activeSelf)
                uiObject.SetActive(true);

            if (powerText != null)
            {
                powerText.text = $"Fuel: {currentFuel}";

                if (currentFuel <= lowFuelThreshold)
                    powerText.color = lowFuelColor;
                else if (currentFuel <= mediumFuelThreshold)
                    powerText.color = mediumFuelColor;
                else
                    powerText.color = normalFuelColor;
            }
        }
        else
        {
            if (uiObject != null && uiObject.activeSelf)
                uiObject.SetActive(false);
        }
    }

    private IEnumerator UpdateUIRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(updateInterval);

        while (true)
        {
            if (isHovering)
                RefreshResourceSlots();
            else
                RefreshFuelUI();

            yield return wait;
        }
    }
}