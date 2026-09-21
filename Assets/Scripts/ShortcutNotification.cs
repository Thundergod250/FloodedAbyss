using UnityEngine;

public static class Notification
{
    public static void Display(string message, Color color)
    {
        if (GameManager.Instance == null || GameManager.Instance.uiController == null)
            return;

        var uiNotification = GameManager.Instance.uiController.uiNotification;
        if (uiNotification != null) 
            uiNotification.ShowNotification(message, color);
    }
    
    public static void ShowWarning(string message) => Display(message, Color.yellow);
}