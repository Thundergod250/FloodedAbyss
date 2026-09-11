using UnityEngine;

public class TemporaryBullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage ;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Health>(out Health goHealth))
        {
            goHealth.TakeDamage(damage);
        }

        Debug.Log("Hit: " + collision.gameObject.name);

        Destroy(gameObject);
    }
}
