using UnityEngine;

public class ObjectMover : Item
{
    [SerializeField] private Vector3 moveOffset = new Vector3(0, 2f, 0);

    public override void Activate()
    {
        transform.position += moveOffset;
        Debug.Log($"{gameObject.name} moved to {transform.position}");
    }
}