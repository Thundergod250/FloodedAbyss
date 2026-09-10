using UnityEngine;
using UnityEngine.Events;

public class Interactables : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private UnityEvent onInteract;

    public virtual void Interact()
    {
        onInteract?.Invoke();
        Debug.Log($"Interacted with {gameObject.name}");
    }
}