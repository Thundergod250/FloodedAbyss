using UnityEngine;

public class VFXFollowCamera : MonoBehaviour
{
    [Header("Axis Tracking Settings")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true; 
    [SerializeField] private bool followZ = true;

    private Transform mainCameraTransform;
    private Vector3 initialPosition;

    private void OnEnable()
    {
        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
        else
            Debug.LogWarning("[VFXFollowCamera] Main Camera not found in the scene!");
        initialPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        Vector3 targetPosition = transform.position;
        Vector3 camPos = mainCameraTransform.position;

        // Conditionally update each axis based on your settings
        targetPosition.x = followX ? camPos.x : initialPosition.x;
        targetPosition.y = followY ? camPos.y : initialPosition.y;
        targetPosition.z = followZ ? camPos.z : initialPosition.z;

        // Apply the updated position
        transform.position = targetPosition;
    }
}