using UnityEngine;

public class UIMovementLockOnActive : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("If you want to lock the Player on Moving")]
    [SerializeField] private bool unlockOnDisable = true;
    
    [Header("Interaction Lock")]
    [Tooltip("If you want to lock the Player Click on the Overworld")]
    [SerializeField] private bool disableClickInteractor = true;

    [SerializeField] private bool showDebugLogs = false;

    private void OnEnable()
    {
        SetMovementEnabled(false);
        SetClickInteractorEnabled(false);

        if (showDebugLogs)
            Debug.Log($"[UIMovementLockOnActive] Locked movement and world interactions because '{gameObject.name}' was enabled.", this);
    }

    private void OnDisable()
    {
        // Check if the application is quitting to prevent null reference errors on teardown
        if (!gameObject.scene.isLoaded) return;

        if (unlockOnDisable)
        {
            SetMovementEnabled(true);
            SetClickInteractorEnabled(true);

            if (showDebugLogs)
                Debug.Log($"[UIMovementLockOnActive] Unlocked movement and world interactions because '{gameObject.name}' was disabled.", this);
        }
    }

    private void SetMovementEnabled(bool isEnabled)
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
            GameManager.Instance.InputManager.SetMovementActive(isEnabled);
        else if (!isEnabled && showDebugLogs) 
            Debug.LogWarning($"[UIMovementLockOnActive] Could not lock movement: GameManager or InputManager is missing.", this);
    }

    private void SetClickInteractorEnabled(bool isEnabled)
    {
        if (!disableClickInteractor) return;
        if (GameManager.Instance != null && GameManager.Instance.ClickInteractor != null)
            GameManager.Instance.ClickInteractor.enabled = isEnabled;
        else if (!isEnabled && showDebugLogs) 
            Debug.LogWarning($"[UIMovementLockOnActive] Could not disable ClickInteractor: GameManager or ClickInteractor reference is missing.", this);
    }
}