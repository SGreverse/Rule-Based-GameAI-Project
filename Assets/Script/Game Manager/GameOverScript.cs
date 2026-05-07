using Assets.SavingSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverScript : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loseScreenCanvas;
    [SerializeField] private Button restartButton;

    private void Awake()
    {
        // Ensure the button is linked to the Restart function
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // Ensure the screen is hidden at the start
        loseScreenCanvas.SetActive(false);
    }

    public void OnPlayerDeath()
    {
        // Show the Lose Screen (The View)
        loseScreenCanvas.SetActive(true);
        
        // Pause the game time so enemies stop moving
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        loseScreenCanvas.SetActive(false);

        //Tell the GameManager to load the save file instead of starting fresh
        PlayerPrefs.SetInt("LoadFromSave", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene("MainGameScene");
    }
}

