using System.Collections.Generic;
using UnityEngine;

public class UIHUD : UiModals
{
    // 1. Enum matching all elements in your HUD hierarchy
    public enum HuDPanels
    {
        PlayerHP,
        Stamina,
        Oxygen,
        InteractionPanel,
        Objective,
        Debug,
        DebugButtons,
        NotificationPanel,
        WaterForecast,
    }

    [System.Serializable]
    public struct HUDElementReference
    {
        public HuDPanels panelsType;
        public GameObject panelObject;
    }

    [Header("HUD References")]
    [SerializeField] private List<HUDElementReference> hudElements;
    private Dictionary<HuDPanels, GameObject> elementDictionary;

    [Header("HUD Elements")] 
    public PlayerUI PlayerUI;
    public UIInteraction  UIInteraction;
    public UIDebug UIDebug;
    public UINotification UINotification;
    
    protected override void Initialize()
    {
        base.Initialize(); 

        elementDictionary = new Dictionary<HuDPanels, GameObject>();

        foreach (var element in hudElements)
        {
            if (element.panelObject != null && !elementDictionary.ContainsKey(element.panelsType)) 
                elementDictionary.Add(element.panelsType, element.panelObject);
        }
    }
    
    public void SetHUDElementActive(HuDPanels panelsType, bool isActive)
    {
        if (elementDictionary != null && elementDictionary.TryGetValue(panelsType, out GameObject panel))
        {
            if (panel != null) 
                panel.SetActive(isActive);
        }
    }
    
    public void ToggleHUDElement(HuDPanels panelsType)
    {
        if (elementDictionary != null && elementDictionary.TryGetValue(panelsType, out GameObject panel))
        {
            if (panel != null) 
                panel.SetActive(!panel.activeSelf);
        }
    }
    
    public T GetHUDElement<T>(HuDPanels panelsType) where T : Component
    {
        if (elementDictionary != null && elementDictionary.TryGetValue(panelsType, out GameObject panel))
        {
            if (panel != null)
            {
                T component = panel.GetComponent<T>();
                if (component != null) return component;

                Debug.LogWarning($"[UIHUD] Component {typeof(T).Name} not found on GameObject for {panelsType}.");
                return null;
            }
        }

        Debug.LogWarning($"[UIHUD] HUD Panel for state {panelsType} not found.");
        return null;
    }
}