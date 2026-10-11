using UnityEngine;

public class UIController : MonoBehaviour
{
    private UiUpgrade uiUpgrade;

    private void Awake()
    {
        uiUpgrade = GetComponent<UiUpgrade>();
        
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

    public void OpenUpgradeHub()
    {
        if (uiUpgrade != null)
        {
            uiUpgrade.SetModalActive(true);
        }
    }

    public void CloseAllModals()
    {
        if (uiUpgrade != null)
        {
            uiUpgrade.SetModalActive(false);
        }
    }
}