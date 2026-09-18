using UnityEngine;

public class MinableRock : MonoBehaviour
{
    [Header("Rock Health")]
    [SerializeField] private int health = 30;

    [Header("Drop Settings")]
    [SerializeField] private GameObject orePrefab;
    [SerializeField] private int oreDropCount = 3;
    [SerializeField] private float dropScatterForce = 3f;

    public void TakeMineDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            BreakAndDropOres();
        }
    }

    private void BreakAndDropOres()
    {
        for (int i = 0; i < oreDropCount; i++)
        {
            if (orePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                GameObject ore = Pool.Instantiate(orePrefab, spawnPos, Quaternion.identity);

                if (ore.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    Vector3 randomDir = Random.insideUnitSphere + Vector3.up;
                    rb.AddForce(randomDir * dropScatterForce, ForceMode.Impulse);
                }
            }
        }

        gameObject.SetActive(false);
    }
}