using UnityEngine;
using UnityEngine.Events;

public class Interactables : MonoBehaviour
{
    [Header("UI Message")]
    public string interactionMessage; 

    [Header("Interaction Settings")]
    [SerializeField] private UnityEvent onInteract;

    private Item item;
    private Structures structure;

    private void Awake() 
    {
        item = GetComponent<Item>();
        structure = GetComponent<Structures>();
    }

    public virtual void Interact()
    {
        onInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");

        // Activate Item if it exists on this GameObject
        if (item != null) 
            item.Activate();

        // Activate Structure if it exists on this GameObject
        if (structure != null) 
            structure.Activate();
    }
}