using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global References")]
    public StructureApexPump ApexPump;
    public UIController uiController;
    public PlayerController playerController;
    public ObjectPooling objectPooling;
    public WaterLevel WaterLevel;
    public InputManager InputManager;
    public CursorManager CursorManager; 
    public ClickInteractor ClickInteractor;
    public WorkerManager WorkerManager;
    public ResourceManager ResourceManager;
    public TimeManager TimeManager;

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