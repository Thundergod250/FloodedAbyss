using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float defaultFixedDeltaTime = 0.02f;

    private float currentTimeScale = 1f;
    private bool isPaused = false;
    private float prePauseTimeScale = 1f;

    private void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        // Hook up to InputManager events
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
        {
            SubscribeInputs(GameManager.Instance.InputManager);
        }
        else
        {
            StartCoroutine(WaitForInputManagerRoutine());
        }
    }

    private System.Collections.IEnumerator WaitForInputManagerRoutine()
    {
        while (GameManager.Instance == null || GameManager.Instance.InputManager == null)
        {
            yield return null;
        }
        SubscribeInputs(GameManager.Instance.InputManager);
    }

    private void SubscribeInputs(InputManager inputMgr)
    {
        inputMgr.OnPauseTime.AddListener(TogglePause);
        inputMgr.OnTime1.AddListener(() => SetTimeSpeed(1f));
        inputMgr.OnTime2.AddListener(() => SetTimeSpeed(1.5f));
        inputMgr.OnTime3.AddListener(() => SetTimeSpeed(2f));
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.InputManager != null)
        {
            var inputMgr = GameManager.Instance.InputManager;
            inputMgr.OnPauseTime.RemoveListener(TogglePause);
            inputMgr.OnTime1.RemoveAllListeners();
            inputMgr.OnTime2.RemoveAllListeners();
            inputMgr.OnTime3.RemoveAllListeners();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            SetTimeSpeed(prePauseTimeScale);
        }
        else
        {
            prePauseTimeScale = currentTimeScale > 0 ? currentTimeScale : 1f;
            SetTimeSpeed(0f);
            isPaused = true;
        }
    }

    public void SetTimeSpeed(float scale)
    {
        currentTimeScale = Mathf.Max(0f, scale);
        Time.timeScale = currentTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        if (currentTimeScale > 0f)
        {
            isPaused = false;
        }
        else
        {
            isPaused = true;
        }

        Debug.Log($"[TimeManager] Time Scale set to: {currentTimeScale}x");
    }
}