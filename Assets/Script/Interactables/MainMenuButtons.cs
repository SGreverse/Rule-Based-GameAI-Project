using Assets.SavingSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of your main gameplay scene as written in Build Settings")]
    [SerializeField] private string mainGameplaySceneName = "MainGameScene";
    public void OnStartGameClicked()
    {
        Debug.Log("Starting a new game...");

        // Ensure the load flag is completely reset for a fresh game
        PlayerPrefs.SetInt("LoadFromSave", 0);
        PlayerPrefs.Save();

        // Load the main gameplay scene
        SceneManager.LoadScene(mainGameplaySceneName);
    }

    /// <summary>
    /// Called by the "Load Save" button.
    /// </summary>
    public void OnLoadSaveClicked()
    {
        Debug.Log("Loading saved game...");

        // Set a flag that the GameManager will read when the scene finishes loading
        PlayerPrefs.SetInt("LoadFromSave", 1);
        PlayerPrefs.Save();

        // Load the main gameplay scene
        SceneManager.LoadScene(mainGameplaySceneName);
    }

    /// <summary>
    /// Called by the "Exit" button. Closes the application.
    /// </summary>
    public void OnExitClicked()
    {
        Debug.Log("Exiting game...");

        // This command works when you build the final .exe for your testers
        Application.Quit();

        // This preprocessor directive makes the button work while testing inside the Unity Editor!
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
