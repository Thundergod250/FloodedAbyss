using UnityEngine;

public class UIController : MonoBehaviour
{
    private UiUpgrade uiUpgrade;
    private UIAutoFarm uiAutoFarm;
    private UiApexPump uiApexPump;
    private UiTrade uiTrade;

    private void Awake()
    {
        uiUpgrade = GetComponent<UiUpgrade>();
        uiAutoFarm = GetComponent<UIAutoFarm>(); 
        uiApexPump = GetComponent<UiApexPump>();
        uiTrade = GetComponent<UiTrade>();
        
        if (uiUpgrade == null)
            Debug.LogWarning("[UIController] UiUpgrade reference is missing!");
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
        {
            GameManager.Instance.InputManager.OnEscape.AddListener(CloseAllModals);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
        {
            GameManager.Instance.InputManager.OnEscape.RemoveListener(CloseAllModals);
        }
    }

    private void CloseAllActiveModalsHelper()
    {
        if (uiUpgrade != null) uiUpgrade.SetModalActive(false);
        if (uiAutoFarm != null) uiAutoFarm.SetModalActive(false);
        if (uiApexPump != null) uiApexPump.SetModalActive(false);
        if (uiTrade != null) uiTrade.SetModalActive(false);
    }

    public void OpenUpgradeHub()
    {
        CloseAllActiveModalsHelper();
        if (uiUpgrade != null) uiUpgrade.SetModalActive(true);
    }
    
    public void OpenAutoFarmHub()
    {
        CloseAllActiveModalsHelper();
        if (uiAutoFarm != null) uiAutoFarm.SetModalActive(true);
    }
    
    public void OpenApexPumpHub()
    {
        CloseAllActiveModalsHelper();
        if (uiApexPump != null) uiApexPump.SetModalActive(true);
    }
    
    public void OpenTradeHub()
    {
        CloseAllActiveModalsHelper();
        if (uiTrade != null) uiTrade.SetModalActive(true);
    }

    public void CloseAllModals()
    {
        CloseAllActiveModalsHelper();
    }
}