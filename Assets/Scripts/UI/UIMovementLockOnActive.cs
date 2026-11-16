using UnityEngine;

public class UIMovementLockOnActive : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("If true, disabling this object will restore movement. Turn this off if another panel is supposed to stay open.")]
    [SerializeField] private bool unlockOnDisable = true;
    [SerializeField] private bool showDebugLogs = false;

    private void OnEnable()
    {
        SetMovementEnabled(false);
        if (showDebugLogs)
            Debug.Log($"[InputLockOnActive] Locked movement because '{gameObject.name}' was enabled.", this);
    }

    private void OnDisable()
    {
        // Check if the application is quitting to prevent null reference errors on teardown
        if (!gameObject.scene.isLoaded) return;

        if (unlockOnDisable)
        {
            SetMovementEnabled(true);
            if (showDebugLogs)
                Debug.Log($"[InputLockOnActive] Unlocked movement because '{gameObject.name}' was disabled.", this);
        }
    }

    private void SetMovementEnabled(bool isEnabled)
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
            GameManager.Instance.InputManager.SetMovementActive(isEnabled);
        else if (!isEnabled && showDebugLogs) 
            Debug.LogWarning($"[InputLockOnActive] Could not lock movement: GameManager or InputManager is missing.", this);
    }
}
