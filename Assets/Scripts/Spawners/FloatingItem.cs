using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    [Header("Floating Settings")]
    public float speed = 20f;
    public float lifetime = 180f;
    public Vector3 moveDirection = Vector3.right;

    private bool isVisible = false;
    private float currentLifetime;

    private void Start()
    {
        // Failsafe: If the spawner forgot to call Initialize, use the inspector values
        if (currentLifetime <= 0f)
        {
            currentLifetime = lifetime;
        }
    }

    // FIX: Matches the Spawner's exact argument order: (Direction, Speed, Lifetime)
    public void Initialize(Vector3 direction, float moveSpeed, float lifeTime)
    {
        moveDirection = direction.normalized;
        speed = moveSpeed;
        lifetime = lifeTime;
        currentLifetime = lifetime;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
    }

    private void Update()
    {
        // 1. Move the object
        transform.position += moveDirection * speed * Time.deltaTime;

        // 2. Tick down the timer
        if (currentLifetime > 0f)
        {
            currentLifetime -= Time.deltaTime;
        }

        // 3. ONLY despawn if the timer is up AND the item is off-screen
        if (currentLifetime <= 0f && !isVisible)
        {
            Destroy(gameObject);
        }

        // 4. Failsafe: Force destroy if it drifts miles away into the void
        if (transform.position.x > 3000f || transform.position.x < -3000f)
        {
            Destroy(gameObject);
        }
    }
}