using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global References")]
    public UIController uiController;
    public PlayerController playerController;
    public ObjectPooling objectPooling;
    public ResourceManager resourceManager;
    public WaterLevel waterLevel;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }
}