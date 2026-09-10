using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public void Collect()
    {
        Debug.Log($"Picked up {gameObject.name}");
        gameObject.SetActive(false);
    }
}