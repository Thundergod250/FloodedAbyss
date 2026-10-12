using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private float lifetimeTimer;
    private bool isInitialized = false;

    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifetimeTimer = lifetime;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Move across the water surface (respects Time.deltaTime, so it freezes when paused!)
        transform.position += moveDirection * (moveSpeed * Time.deltaTime);

        // Handle auto-despawn timer if left uncollected
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}