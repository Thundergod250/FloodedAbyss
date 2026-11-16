using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;

public class UIMainMenu : MonoBehaviour
{
    // Must match the child GameObject names on the SoundManager prefab.
    private const string UnderwaterSFXName = "SFXUnderwater";
    private const string OceanSFXName = "SFXOcean";

    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject mainUI;

    [Header("Camera References")]
    [SerializeField] private GameObject menuCameraGameObject;
    [SerializeField] private GameObject gameplayCameraGameObject;
    [SerializeField] private CinemachineBrain cinemachineBrain;

    [Header("Gameplay Script Reference")]
    [SerializeField] private MonoBehaviour cameraMovementScript;

    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // Ensure proper starting states for the menu
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (mainUI != null) mainUI.SetActive(false);

        if (cameraMovementScript != null) cameraMovementScript.enabled = false;

        // Ensure Menu Camera is ON and Gameplay Camera is OFF at start
        if (menuCameraGameObject != null) menuCameraGameObject.SetActive(true);
        if (gameplayCameraGameObject != null) gameplayCameraGameObject.SetActive(false);

        if (cinemachineBrain == null && Camera.main != null)
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        // ── Menu ambience: underwater ON, ocean OFF ──────────────────
        ApplyMenuAmbience();
        // ─────────────────────────────────────────────────────────────
    }

    private void OnStartClicked()
    {
        // ── Gameplay ambience: underwater OFF, ocean ON ──────────────
        ApplyGameplayAmbience();
        // ─────────────────────────────────────────────────────────────

        // 1. Hide the Main Menu UI immediately
        if (mainMenuUI != null) mainMenuUI.SetActive(false);

        // 2. Activate Gameplay Camera and deactivate Menu Camera to force the Cinemachine blend
        if (gameplayCameraGameObject != null) gameplayCameraGameObject.SetActive(true);
        if (menuCameraGameObject != null) menuCameraGameObject.SetActive(false);

        // 3. Wait for the transition to finish before enabling Main UI and Camera Movement
        StartCoroutine(WaitForCameraTransitionRoutine());
    }

    private IEnumerator WaitForCameraTransitionRoutine()
    {
        if (cinemachineBrain != null)
        {
            // Wait a frame for the blend to register as active
            yield return null;

            // While Cinemachine is actively blending between cameras, pause here
            while (cinemachineBrain.IsBlending)
            {
                yield return null;
            }
        }
        else
        {
            // Fallback flat wait if reference is missing
            yield return new WaitForSeconds(2f);
        }

        // 4. AFTER transition is complete, enable Main UI and Camera Movement
        if (mainUI != null) mainUI.SetActive(true);
        if (cameraMovementScript != null) cameraMovementScript.enabled = true;

        Debug.Log("[UIMainMenu] Camera transition complete. Gameplay UI and movement activated.");
    }

    private void OnExitClicked()
    {
        // Stop both ambience loops before quitting so they don't trail off
        // on the last frame of Application.Quit() in the editor.
        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm != null)
        {
            sm.Stop(UnderwaterSFXName);
            sm.Stop(OceanSFXName);
        }

        Debug.Log("[UIMainMenu] Quitting application...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>
    /// Menu-state ambience: underwater loop plays, ocean loop is silenced.
    /// </summary>
    private void ApplyMenuAmbience()
    {
        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm == null) return;

        sm.Stop(OceanSFXName);
        sm.Play(UnderwaterSFXName, true); // loop
    }

    /// <summary>
    /// Gameplay-state ambience: underwater loop stops, ocean loop plays.
    /// </summary>
    private void ApplyGameplayAmbience()
    {
        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm == null) return;

        sm.Stop(UnderwaterSFXName);
        sm.Play(OceanSFXName, true); // loop
    }
}