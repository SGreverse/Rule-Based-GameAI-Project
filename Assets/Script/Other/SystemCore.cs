using UnityEngine;

public class SystemCore : MonoBehaviour
{
    private void Awake()
    {
        // count how many SystemCores exist in the scene right now
        SystemCore[] existingCores = Object.FindObjectsByType<SystemCore>(FindObjectsSortMode.None);


        if (existingCores.Length > 1)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
