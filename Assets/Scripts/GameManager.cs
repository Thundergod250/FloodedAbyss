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
    public StructureProgression structureProgression;
    public TideManager TideManager;
    public SoundManager SoundManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        ResolveTideManager();
    }
    public void Start()
    {
        GameManager.Instance.SoundManager.Play("SFXOcean");
    }

    private void ResolveTideManager()
    {
        if (TideManager == null)
            TideManager = GetComponent<TideManager>();

        if (TideManager == null)
            TideManager = FindAnyObjectByType<TideManager>(FindObjectsInactive.Include);

        if (TideManager != null && !TideManager.gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "[GameManager] TideManager found on INACTIVE GameObject '" +
                TideManager.gameObject.name +
                "'. Tide cycles will not run. " +
                "Move the TideManager component to an active GameObject " +
                "(e.g. this GameManager object) and clear the reference on " +
                "the WaterLevel object so there aren't two instances."
            );
        }
        else if (TideManager == null)
        {
            Debug.LogWarning(
                "[GameManager] No TideManager in the scene. " +
                "High/low tide mechanics are disabled."
            );
        }
    }
    private void ResolveSoundManager()
    {
        if (SoundManager == null)
            SoundManager = GetComponent<SoundManager>();

        if (SoundManager == null)
            SoundManager = FindAnyObjectByType<SoundManager>(FindObjectsInactive.Include);

        if (SoundManager == null)
        {
            Debug.LogWarning(
                "[GameManager] No SoundManager found in the scene. " +
                "Sound playback will be disabled."
            );
        }
    }

}