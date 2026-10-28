using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Start()
    {
        // Ensure cursor starts visible and unlocked for RTS gameplay
        SetCursorVisibility(true);
    }

    // Controls cursor visibility and lock state
    public void SetCursorVisibility(bool showCursor)
    {
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }
}