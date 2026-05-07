using Assets.Algorithm.BlackBoard;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDoor : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The exact name of your Boss Arena scene in the Build Settings")]
    public string BossSceneName = "BossArenaScene";

    [Tooltip("The text to send to the UI when collected")]
    public UINotificationPayload notificationData;

    [Tooltip("Event that fires and passes the string to the UI")]
    public StringEventChannelSO uiNotificationChannel;

    private bool _isOpen = false;
    
    public void OpenDoor()
    {
        this._isOpen = true;
    }
    public void LockDoor()
    {
        this._isOpen=false; 
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object colliding is the Player
        PlayerManager player = other.GetComponent<PlayerManager>();

        if (player != null)
        {
            //  Check if the door is open
            if (_isOpen)
            {
                Debug.Log("<color=green>The Boss Door opens! Transitioning to Boss Room...</color>");
                LoadBossRoom(player);
            }
            else
            {
                // Calculate how many keys are missing for a helpful debug message
                int missingKeys = GameManager.Instance.TotalKeysRequired - GameManager.Instance.KeysCollected;
                notificationData.Message = $"The door is firmly locked. You need {missingKeys} more key(s)";
                uiNotificationChannel.RaiseEvent(notificationData);
            }
        }
    }

    private void LoadBossRoom(PlayerManager player)
    {
        // This command tells Unity to destroy the current scene and load the new one.
        // (Remember, our GameManager has DontDestroyOnLoad, so it will survive the trip!)
        GameBlackboard.Instance.ResetBlackboard();
        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onSceneLoaded = null;
        onSceneLoaded = (scene, mode) =>
        {
            // Make sure we only teleport if the scene that loaded is the Boss Room
            if (scene.name == BossSceneName)
            {
                player.transform.position = Vector2.zero;

                // Clean up the listener so it doesn't trigger again later
                SceneManager.sceneLoaded -= onSceneLoaded;
            }
        };
        SceneManager.sceneLoaded += onSceneLoaded;

        SceneManager.LoadScene(BossSceneName);
    }
}
