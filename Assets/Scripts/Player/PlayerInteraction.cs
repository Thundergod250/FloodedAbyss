using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Input Stuff")]
    [SerializeField] private PlayerInput input;
    [SerializeField] private InputAction useAction;
    [SerializeField] private InputAction interactAction;

    [Header("UI")]
    [SerializeField] private GameObject interactUI;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        useAction = input.actions.FindAction("Use");
        interactAction = input.actions.FindAction("Interact");
    }
    private void OnEnable()
    {
        useAction.started += UseItem;
        interactAction.started += Interact;
    }

    private void OnDisable()
    {
        useAction.started -= UseItem;
        interactAction.started -= Interact;
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
