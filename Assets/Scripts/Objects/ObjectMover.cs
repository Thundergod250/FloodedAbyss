using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset = new Vector3(0, 2f, 0);

    public void MoveObject()
    {
        transform.position += moveOffset;
        Debug.Log($"{gameObject.name} moved to {transform.position}");
    }
}