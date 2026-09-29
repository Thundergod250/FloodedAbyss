using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStructureCard : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI structureNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Image References")]
    [SerializeField] private Image structureIconImage;

    [Header("Cost Display Setup")]
    [SerializeField] private Transform costContainer; 
    [SerializeField] private TextMeshProUGUI costTextPrefab; 

    [Header("Button Reference")]
    [SerializeField] private Button buildButton;

    [Header("Lock Overlay Setup")]
    [SerializeField] private GameObject lockOverlayObject;
    
    public void SetupCard(string name, string description, List<string> costs, Sprite icon, bool isUnlocked, Action onBuildClicked = null)
    {
        if (structureNameText != null) 
            structureNameText.text = name;

        if (descriptionText != null) 
            descriptionText.text = description;

        if (structureIconImage != null)
        {
            structureIconImage.sprite = icon;
            structureIconImage.gameObject.SetActive(icon != null);
        }

        PopulateCosts(costs);

        // Toggle the lock overlay image (enabled when locked, disabled when unlocked)
        if (lockOverlayObject != null)
        {
            lockOverlayObject.SetActive(!isUnlocked);
        }

        // Setup Build Button Listener & Interactivity
        if (buildButton != null)
        {
            buildButton.interactable = isUnlocked;
            buildButton.onClick.RemoveAllListeners();

            if (isUnlocked && onBuildClicked != null)
            {
                buildButton.onClick.AddListener(() => onBuildClicked.Invoke());
            }
        }
    }

    private void PopulateCosts(List<string> costs)
    {
        if (costContainer == null || costTextPrefab == null) return;
        
        foreach (Transform child in costContainer) 
            Pool.Destroy(child.gameObject);
        
        if (costs != null)
        {
            foreach (string cost in costs)
            {
                TextMeshProUGUI costItem = Pool.Instantiate(costTextPrefab, costContainer);
                costItem.text = cost; 
            }
        }
    }
}