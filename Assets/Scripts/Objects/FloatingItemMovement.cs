using UnityEngine;

public class FloatingItemMovement : MonoBehaviour
{
    [Header("Bobbing Settings (Up & Down)")]
    [SerializeField] private float verticalAmplitude = 0.5f; 
    [SerializeField] private float verticalSpeed = 1.5f;     

    [Header("Swaying Settings (Omnidirectional Drift & Tilt)")]
    [SerializeField] private float horizontalAmplitude = 0.2f; 
    [SerializeField] private float horizontalSpeed = 1.0f;     
    [SerializeField] private float tiltAngle = 5.0f;           

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private float randomOffset;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        randomOffset = transform.position.x * 0.5f + transform.position.z * 0.5f;
    }

    private void Update()
    {
        float time = Time.time + randomOffset;
        float verticalOffset = Mathf.Sin(time * verticalSpeed) * verticalAmplitude;
        float horizontalOffsetX = Mathf.Cos(time * horizontalSpeed * 0.5f) * horizontalAmplitude;
        float horizontalOffsetZ = Mathf.Sin(time * horizontalSpeed * 0.7f) * horizontalAmplitude;
        transform.localPosition = startLocalPosition + new Vector3(horizontalOffsetX, verticalOffset, horizontalOffsetZ);
        float tiltZ = Mathf.Sin(time * horizontalSpeed) * tiltAngle;
        float tiltX = Mathf.Cos(time * horizontalSpeed * 0.8f) * tiltAngle;
        transform.localRotation = startLocalRotation * Quaternion.Euler(tiltX, 0f, tiltZ);
    }
}