using System.Collections;
using UnityEngine;

public class FadeInManager : MonoBehaviour
{
    [Header("Logo Animation Settings")]
    [Tooltip("The RectTransform component of the Logo Image")]
    [SerializeField] private RectTransform logoRectTransform;

    [Tooltip("The CanvasGroup component attached to the Logo")]
    [SerializeField] private CanvasGroup logoCanvasGroup;

    [Tooltip("How long to wait in seconds BEFORE the animation starts")]
    [SerializeField] private float startDelay = 0.5f;

    [Tooltip("How long the animation should take in seconds")]
    [SerializeField] private float animationDuration = 1.5f;

    [Tooltip("How many pixels down the logo should start before floating up")]
    [SerializeField] private float startOffsetPixels = 100f;

    private void Start()
    {
        // Start the animation as soon as the Main Menu scene loads
        if (logoRectTransform != null && logoCanvasGroup != null)
        {
            StartCoroutine(AnimateLogoRoutine());
        }
        else
        {
            Debug.LogWarning("MainMenuManager: Missing Logo references!");
        }
    }

    /// <summary>
    /// Coroutine to smoothly fade in and float the logo upwards.
    /// </summary>
    private IEnumerator AnimateLogoRoutine()
    {
        // 1. Setup Initial State
        // Record the final position we want the logo to rest at (where you placed it in the Editor)
        Vector2 finalPosition = logoRectTransform.anchoredPosition;

        // Calculate the starting position (moved down by the offset)
        Vector2 startPosition = finalPosition - new Vector2(0f, startOffsetPixels);

        // Apply the starting state immediately so it's hidden on frame 1
        logoRectTransform.anchoredPosition = startPosition;
        logoCanvasGroup.alpha = 0f;

        if (startDelay > 0f)
        {
            // This pauses the Coroutine here for the specified amount of seconds
            yield return new WaitForSeconds(startDelay);
        }

        // 2. Animate over time
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            // Calculate a percentage (0.0 to 1.0) of how far along the animation is
            float t = elapsedTime / animationDuration;

            // Smooth step adds a nice ease-in/ease-out effect so the animation isn't rigidly linear
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            // Interpolate the position and the alpha
            logoRectTransform.anchoredPosition = Vector2.Lerp(startPosition, finalPosition, smoothStep);
            logoCanvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothStep);

            // Increment time
            elapsedTime += Time.deltaTime;

            // Wait until the next frame before continuing the loop
            yield return null;
        }

        // 3. Ensure Final State is perfect
        // To prevent floating point inaccuracies, hard-set the final values at the end
        logoRectTransform.anchoredPosition = finalPosition;
        logoCanvasGroup.alpha = 1f;
    }
}
