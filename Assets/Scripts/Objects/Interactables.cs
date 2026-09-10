using UnityEngine;

public class Interactables : MonoBehaviour
{
    public virtual void Interact()
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }
}