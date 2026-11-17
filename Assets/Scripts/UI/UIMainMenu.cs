using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;

public class UIMainMenu : MonoBehaviour
{
    private const string UnderwaterSFXName = "SFXUnderwater";
    private const string OceanSFXName = "SFXOcean";
    private const string WooshSFXName = "SFXWoosh";

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

    [Header("Ambience Crossfade")]
    [Tooltip("Seconds to crossfade between the underwater menu loop and the ocean gameplay loop.")]
    [SerializeField] private float ambienceCrossfadeDuration = 1.5f;

    // Captured right after Play("SFXOcean", true) so we know the master-volume-
    // scaled target the ocean loop should end up at. SoundManager.Play() sets
    // AudioSource.volume = inspectorVolume * masterVolume on every call, so
    // this is the only reliable way for UIMainMenu to respect master volume.
    private float oceanTargetVolume = 1f;

    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (mainUI != null) mainUI.SetActive(false);

        if (cameraMovementScript != null) cameraMovementScript.enabled = false;

        if (menuCameraGameObject != null) menuCameraGameObject.SetActive(true);
        if (gameplayCameraGameObject != null) gameplayCameraGameObject.SetActive(false);

        if (cinemachineBrain == null && Camera.main != null)
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        PrepareMenuAmbience();
    }

    private void OnStartClicked()
    {
        // ── Woosh: one-shot transition sting ────────────────────────
        // Fires on the click frame, alongside the crossfade. SoundManager
        // handles the auto-disable after the clip's length.
        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm != null)
            sm.Play(WooshSFXName);
        // ─────────────────────────────────────────────────────────────

        StartCoroutine(CrossfadeAmbienceToGameplay());

        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (gameplayCameraGameObject != null) gameplayCameraGameObject.SetActive(true);
        if (menuCameraGameObject != null) menuCameraGameObject.SetActive(false);

        StartCoroutine(WaitForCameraTransitionRoutine());
    }

    private IEnumerator WaitForCameraTransitionRoutine()
    {
        if (cinemachineBrain != null)
        {
            yield return null;
            while (cinemachineBrain.IsBlending) yield return null;
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        if (mainUI != null) mainUI.SetActive(true);
        if (cameraMovementScript != null) cameraMovementScript.enabled = true;

        Debug.Log("[UIMainMenu] Camera transition complete. Gameplay UI and movement activated.");
    }

    private void OnExitClicked()
    {
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

    // ─────────────────────────────────────────────────────────────────────
    //  Ambience
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Menu state: underwater loop at full volume, ocean loop playing but silent.
    ///
    /// Note: SoundManager.InitializeSoundSources() briefly activates then
    /// deactivates every child AudioSource in Awake(), so PlayOnAwake does
    /// NOT leave anything playing by the time Start() runs. Both loops must
    /// be explicitly Play()'d here.
    /// </summary>
    private void PrepareMenuAmbience()
    {
        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm == null) return;

        var underwater = FindSFXAudioSource(sm, UnderwaterSFXName);
        var ocean = FindSFXAudioSource(sm, OceanSFXName);

        // Underwater: Play() sets volume = inspectorVolume * masterVolume.
        // We leave that value intact — it's the "on" level for the menu.
        if (underwater != null && !underwater.isPlaying)
            sm.Play(UnderwaterSFXName, true);

        // Ocean: Play() activates the GameObject and starts the loop at full
        // natural volume. Capture that value (so the crossfade respects
        // masterVolume), then silence the source. Same-frame volume=0 means
        // no audible blip: the audio thread hasn't consumed a buffer yet.
        if (ocean != null)
        {
            if (!ocean.isPlaying)
                sm.Play(OceanSFXName, true);

            oceanTargetVolume = ocean.volume;
            ocean.volume = 0f;
        }
    }

    /// <summary>
    /// Fades underwater → 0 while fading ocean 0 → oceanTargetVolume over
    /// ambienceCrossfadeDuration, then stops the silent underwater loop.
    /// Both start values are the master-volume-scaled volumes SoundManager
    /// produced when it last Play()'d each source, so this works correctly
    /// at any masterVolume setting.
    /// </summary>
    private IEnumerator CrossfadeAmbienceToGameplay()
    {
        var sm = GameManager.Instance != null ? GameManager.Instance.SoundManager : null;
        if (sm == null) yield break;

        var underwater = FindSFXAudioSource(sm, UnderwaterSFXName);
        var ocean = FindSFXAudioSource(sm, OceanSFXName);

        // Fallback: if the AudioSources can't be found (child renamed, etc.),
        // do a hard swap rather than leaving the player with no ambience.
        if (underwater == null || ocean == null)
        {
            Debug.LogWarning("[UIMainMenu] Could not find ambience AudioSources — hard swapping.");
            sm.Stop(UnderwaterSFXName);
            sm.Play(OceanSFXName, true);
            yield break;
        }

        // Ensure both loops are actually playing. If either was stopped
        // externally (or by a Stop call), re-Play and re-capture.
        if (!underwater.isPlaying)
            sm.Play(UnderwaterSFXName, true);

        if (!ocean.isPlaying)
        {
            sm.Play(OceanSFXName, true);
            oceanTargetVolume = ocean.volume; // re-capture if replayed
        }

        // Capture current underwater volume (already masterVolume-scaled by Play).
        float underwaterStart = underwater.volume;

        // Force known start states: ocean silent, underwater at its play level.
        // Same-frame adjustments only — the audio thread hasn't consumed a
        // buffer yet, so neither change is audible.
        ocean.volume = 0f;

        float elapsed = 0f;
        while (elapsed < ambienceCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / ambienceCrossfadeDuration);

            underwater.volume = Mathf.Lerp(underwaterStart, 0f, alpha);
            ocean.volume = Mathf.Lerp(0f, oceanTargetVolume, alpha);

            yield return null;
        }

        // Snap to final values.
        underwater.volume = 0f;
        ocean.volume = oceanTargetVolume;

        // Deactivate the silent underwater source via SoundManager so its
        // GameObject is set inactive (Stop also stops playback).
        sm.Stop(UnderwaterSFXName);
    }

    /// <summary>
    /// Finds the AudioSource under SoundManager whose GameObject matches
    /// the given SFX name (matching the child-GameObject naming convention
    /// visible on the SoundManager prefab).
    /// </summary>
    private static AudioSource FindSFXAudioSource(SoundManager sm, string childName)
    {
        if (sm == null || string.IsNullOrEmpty(childName)) return null;
        Transform t = sm.transform.Find(childName);
        return t != null ? t.GetComponent<AudioSource>() : null;
    }
}