using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Script.Game_Manager
{
    internal class WinScript:MonoBehaviour
    {

        [Header("UI References")]
        [SerializeField] private GameObject WinCanvas;
        [SerializeField] private Button CloseGameButton;

        private void Awake()
        {
            if (CloseGameButton != null)
            {
                CloseGameButton.onClick.AddListener(OnExitClicked);
            }

            WinCanvas.SetActive(false);
        }

        public void OnPlayerWon()
        {
            WinCanvas.SetActive(true);

            // Pause the game time so no one moves
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void OnExitClicked()
        {
            Debug.Log("Exiting game...");

           
            Application.Quit();

            // This preprocessor directive makes the button work while testing inside the Unity Editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
