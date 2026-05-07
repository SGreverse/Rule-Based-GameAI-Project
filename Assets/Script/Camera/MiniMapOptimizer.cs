using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapOptimizer : MonoBehaviour
{
    private Camera _minimapCamera;

    [Tooltip("How many times per second the minimap updates. Lower is better for CPU.")]
    [Range(1f, 30f)]
    public float updatesPerSecond = 10f;

    private WaitForSeconds _renderWait;

    void Start()
    {
        _minimapCamera = GetComponent<Camera>();

        // Disable auto-rendering to save performance
        _minimapCamera.enabled = false;

        // Cache the WaitForSeconds object to prevent Garbage Collection allocation every loop
        _renderWait = new WaitForSeconds(1f / updatesPerSecond);

        // Start the controlled rendering loop
        StartCoroutine(RenderMinimapRoutine());
    }

    /// <summary>
    /// A coroutine that acts as a controlled loop, rendering the camera manually 
    /// at the specified interval.
    /// </summary>
    private IEnumerator RenderMinimapRoutine()
    {
        while (true)
        {
            // Wait for the specified interval before executing again
            yield return _renderWait;

            // Optional: Wait until the end of the frame so rendering the minimap
            // doesn't block the main game logic (Movement, AI, etc.)
            yield return new WaitForEndOfFrame();

            // Force the camera to draw. 
            // CRITICAL: Ensure this camera's Culling Mask is set to ONLY render simple UI/Minimap layers!
            if (_minimapCamera != null)
            {
                _minimapCamera.Render();
            }
        }
    }
    private Coroutine _renderCoroutine;

    /// <summary>
    /// Called by the MiniMapController to turn the rendering loop on or off.
    /// This is the core of our CPU optimization strategy.
    /// </summary>
    public void SetRenderingEnabled(bool isEnabled)
    {
        if (isEnabled)
        {
            // Start the rendering loop ONLY if it's not already running
            if (_renderCoroutine == null)
            {
                _renderCoroutine = StartCoroutine(RenderMinimapRoutine());
            }
        }
        else
        {
            // The map is closed. Stop the loop entirely so the CPU can rest!
            if (_renderCoroutine != null)
            {
                StopCoroutine(_renderCoroutine);
                _renderCoroutine = null;
            }
        }
    }
}