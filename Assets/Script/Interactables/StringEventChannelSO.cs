using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A ScriptableObject that acts as an Event Bus for string payloads.
/// Because it is a ScriptableObject, Prefabs can reference it directly!
/// </summary>
[CreateAssetMenu(menuName = "Events/String Event Channel")]
public class StringEventChannelSO : ScriptableObject
{
    // The actual event that listeners will subscribe to
    public event UnityAction<UINotificationPayload> OnEventRaised;
    /// <summary>
    /// Called by the publisher (e.g., Key or Save Station) to broadcast a message.
    /// </summary>
    /// <param name="value">The message payload to pass along</param>
    public void RaiseEvent(UINotificationPayload payload)
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke(payload);
        }
        else
        {
            Debug.LogWarning("An event was raised, but no one is listening!");
        }
    }
}