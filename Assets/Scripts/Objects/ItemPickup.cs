using UnityEngine;

public class ItemPickup : Item
{
    public override void Activate()
    {
        Debug.Log($"Picked up {gameObject.name}");
        Pool.Destroy(gameObject);
    }
}