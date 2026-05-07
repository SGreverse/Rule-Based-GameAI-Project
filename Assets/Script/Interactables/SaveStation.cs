using Assets.SavingSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class Saving : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI GameObject showing 'Press E to Save'")]
    [SerializeField] private GameObject promptUI;

    [Tooltip("The text to send to the UI when collected")]
    public UINotificationPayload notificationData;

    [Tooltip("Event that fires and passes the string to the UI")]
    public StringEventChannelSO uiNotificationChannel;

    private bool isPlayerInRange = false;

    private void Start()
    {
        // Ensure the prompt is hidden when the game starts
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // If the player is in range and presses the interact key (E)
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteSave();
        }
    }

    /// <summary>
    /// Triggers the actual saving logic.
    /// </summary>
    private void ExecuteSave()
    {
        uiNotificationChannel.RaiseEvent(notificationData);

        SaveSystem.Save();

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (promptUI != null)
            {
                promptUI.SetActive(true); // Show the interaction prompt
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the Player left the trigger area
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false); // Hide the prompt
            }
        }
    }
}
