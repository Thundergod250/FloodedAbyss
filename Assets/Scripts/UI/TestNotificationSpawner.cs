using UnityEngine;

public class TestNotificationSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 0.7f;
    private float timer;
    private int counter = 1;

    private readonly Color[] colors = new Color[] 
    { 
        Color.green, 
        Color.red, 
        Color.yellow, 
        Color.cyan 
    };

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;

            Color randomColor = colors[Random.Range(0, colors.Length)];
            string message = $"+{counter * 10} Resource Item #{counter}";

            if (UINotification.Instance != null) 
                UINotification.Instance.ShowNotification(message, randomColor);

            counter++;
        }
    }
}