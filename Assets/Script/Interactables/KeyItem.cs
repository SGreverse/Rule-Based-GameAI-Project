using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyItem : MonoBehaviour
{
    [Tooltip("Check this if you want the key to spin for visual flair")]
    public bool RotateKey = true;
    public float RotationSpeed = 100f;

    [Tooltip("The text to send to the UI when collected")]
    public UINotificationPayload notificationData;

    [Tooltip("Event that fires and passes the string to the UI")]
    public StringEventChannelSO uiNotificationChannel;

    private void Update()
    {
        if (RotateKey)
        {
            transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerManager>() != null)
        {
            uiNotificationChannel.RaiseEvent(notificationData);

            GameManager.Instance.AddKey();

            Destroy(this.gameObject);
        }
    }
}