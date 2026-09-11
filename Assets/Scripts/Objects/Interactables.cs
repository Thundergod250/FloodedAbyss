using UnityEngine;
using UnityEngine.Events;

public class Interactables : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private UnityEvent onInteract;

    private Item item;

    private void Awake()
    {
        item = GetComponent<Item>();
    }

    public virtual void Interact()
    {

        onInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");

        if (item != null)
        {
            item.Activate();
        }
    }
}