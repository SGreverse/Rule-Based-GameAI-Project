using UnityEngine;

/// <summary>
/// A Data Transfer Object (DTO) that holds all the information 
/// needed for a UI Notification. 
/// [System.Serializable] tells Unity to draw this inside the Inspector.
/// </summary>
[System.Serializable]
public struct UINotificationPayload
{
    [Tooltip("The text that will be displayed on the screen")]
    public string Message;

    [Tooltip("The color of the text")]
    public Color TextColor;

    // Notice how scalable this is! If you later want to add an icon:
    // public Sprite NotificationIcon; 
    // You just add it here, and you don't have to rewrite your Event architecture!
}