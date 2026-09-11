using UnityEngine;
using UnityEngine.Events;

public class Interactables : MonoBehaviour
{
    [SerializeField] private Item item;

    private void Awake()
    {
        if (item == null)
        {
            item = GetComponent<Item>();
        }
    }

    public virtual void Interact()
    {
        if (item != null)
        {
            item.Activate();
        }
        else
        {
            Debug.Log($"Interacted with {gameObject.name}");
        }
    }
}