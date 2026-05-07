using TMPro;
using UnityEngine;

public class KeyTracker : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI keyText;

    [Header("Settings")]
    public int totalKeys = 3;
    private int keysCollected = -1;


    // Call this method whenever the player touches a key!
    private void Update()
    {
        if (keysCollected != GameManager.Instance.KeysCollected)
        {
            keysCollected = GameManager.Instance.KeysCollected;
            keyText.text = keysCollected + " / " + totalKeys;
        }
    }


}
