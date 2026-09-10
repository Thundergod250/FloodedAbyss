using UnityEngine;

public class TemporaryBullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hit: " + collision.gameObject.name);

        Destroy(gameObject);
    }
}
