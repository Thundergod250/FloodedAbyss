using UnityEngine;

public class MineTransform : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;

    private void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }
}
