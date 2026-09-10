using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Input References")]
    [SerializeField] private PlayerController controller;

    [Header("UI")]
    [SerializeField] private GameObject interactUI;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (controller != null)
        {
            controller.UseAction.started += UseItem;
            controller.InteractAction.started += Interact;
        }
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.UseAction.started -= UseItem;
            controller.InteractAction.started -= Interact;
        }
    }
    private void UseItem(InputAction.CallbackContext context)
    {
        Debug.Log("Use Item!"); // Put condition if something is held 
    }

    private void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("Interacted with item"); // Put condition if looking at item. Raycast?
        if (interactUI.activeSelf)
        {
            interactUI.SetActive(false);
        }
        else
        {
            interactUI.SetActive(true);
        }
    }

}
