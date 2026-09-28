using UnityEngine;

public class UnderwaterResource : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;
    
    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        Pool.Destroy(gameObject, lifetime);
    }

    private void Update() => transform.position += moveDirection * (moveSpeed * Time.deltaTime);

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("DebrisCleanup"))
        {
            Pool.Destroy(gameObject);
        }*/
    }
}
