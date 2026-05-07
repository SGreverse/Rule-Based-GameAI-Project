using UnityEngine;

public class MiniMapController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the MinimapDisplay GameObject (Panel) here")]
    public GameObject minimapPanel;

    [Tooltip("Drag the object holding the MinimapOptimizer script here")]
    [SerializeField] private MinimapOptimizer _minimapOptimizer;

    [Header("Settings")]
    public KeyCode mapKey = KeyCode.M;

    void Start()
    {
        // 1. Ensure the map UI starts completely closed
        if (minimapPanel != null)
        {
            minimapPanel.SetActive(false);
        }

        // 2. Ensure the camera rendering loop is halted on startup
        if (_minimapOptimizer != null)
        {
            _minimapOptimizer.SetRenderingEnabled(false);
        }
    }

    void Update()
    {
        // Toggle the map when the designated key is pressed
        if (Input.GetKeyDown(mapKey))
        {
            ToggleMap();
        }
    }

    /// <summary>
    /// Handles the logic for opening and closing the map screen.
    /// By separating this into its own method, it can easily be called by UI UI Buttons later if needed.
    /// </summary>
    private void ToggleMap()
    {
        // Safety check to prevent NullReferenceExceptions
        if (minimapPanel == null || _minimapOptimizer == null)
        {
            Debug.LogWarning("[MiniMapController] Missing references! Please assign them in the Inspector.");
            return;
        }

        // Determine the new state (if it was active, it becomes inactive, and vice versa)
        bool isNowOpen = !minimapPanel.activeSelf;

        // Toggle the UI Panel visibility
        minimapPanel.SetActive(isNowOpen);

        // Command the Optimizer to start or stop the rendering loop to save CPU!
        _minimapOptimizer.SetRenderingEnabled(isNowOpen);
    }
}