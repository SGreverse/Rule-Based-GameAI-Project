using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //install 2D pixel perfect package for no pixel jittering problems

    [Header("Target")]
    public Transform playerTransform;

    [Header("Settings")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    public Vector3 offset = new Vector3(0, 0, -10);//camera positioning to record the player away from him
    void Start()
    {
        
    }

    void LateUpdate()
    {
        //only follow the player if he exists
        if (playerTransform == null) return;

        Vector3 desiredPosition = playerTransform.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);//move towards the player at a smooth speed

        transform.position = smoothedPosition;
    }
}
