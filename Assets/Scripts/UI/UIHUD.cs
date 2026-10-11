using UnityEngine;
using TMPro;
using System.Collections;

public class UIHUD : UiModals
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI workerNumberText; // Drag your "Worker Number" UI element here

    private bool isSubscribed = false;

    protected override void Initialize()
    {
        base.Initialize(); 
    }

    private void OnEnable()
    {
        StartCoroutine(ConnectToWorkerManagerRoutine());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator ConnectToWorkerManagerRoutine()
    {
        // Wait until GameManager and WorkerManager exist in the scene
        while (GameManager.Instance == null || GameManager.Instance.WorkerManager == null)
        {
            yield return null; // Wait for the next frame
        }

        Unsubscribe(); // Prevent duplicate listeners

        GameManager.Instance.WorkerManager.EvtOnWorkersChanged.AddListener(HandleWorkersChanged);
        isSubscribed = true;

        // Force an immediate sync with current values
        HandleWorkersChanged(
            GameManager.Instance.WorkerManager.CurrentWorker, 
            GameManager.Instance.WorkerManager.MaximumWorker
        );
    }

    private void Unsubscribe()
    {
        if (isSubscribed && GameManager.Instance != null && GameManager.Instance.WorkerManager != null)
        {
            GameManager.Instance.WorkerManager.EvtOnWorkersChanged.RemoveListener(HandleWorkersChanged);
            isSubscribed = false;
        }
    }

    private void HandleWorkersChanged(int current, int maximum)
    {
        if (workerNumberText != null)
        {
            workerNumberText.text = $"{current} / {maximum}";
        }
    }
}