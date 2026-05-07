using TMPro;
using UnityEngine;

public class FPSScript : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Assign a UI Text element here
    private float deltaTime = 0.0f;

    void Update()
    {
        // Calculate the time it took to complete the last frame
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Calculate FPS: 1 / time per frame
        float fps = 1.0f / deltaTime;

        // Update the View
        fpsText.text = string.Format("{0:0.} FPS", fps);
    }
}
