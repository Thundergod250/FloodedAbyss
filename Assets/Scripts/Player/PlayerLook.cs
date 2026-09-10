using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // Assign your Camera here
    
    [Header("Settings")]
    public float sensitivity = 2f;
    public float pitchClamp = 80f;

    private PlayerController controller;
    private float xRotation = 0f; // Vertical rotation

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }
    

    private void Update()
    {
        if (controller == null || controller.LookAction == null) return;

        Vector2 lookInput = controller.LookAction.ReadValue<Vector2>() * (sensitivity * Time.deltaTime);

        // Horizontal rotation (yaw)
        transform.Rotate(Vector3.up * lookInput.x);

        // Vertical rotation (pitch)
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -pitchClamp, pitchClamp);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
