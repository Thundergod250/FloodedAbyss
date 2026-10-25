using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    // Dictionary mapping sound names to their corresponding AudioSources on child objects
    private Dictionary<string, AudioSource> soundSources = new Dictionary<string, AudioSource>();

    // Tracks active disable coroutines for non-looping sounds so they don't conflict
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        InitializeSoundSources();
    }

    /// <summary>
    /// Automatically finds and maps all AudioSource components on immediate child objects.
    /// </summary>
    private void InitializeSoundSources()
    {
        soundSources.Clear();
        AudioSource[] childSources = GetComponentsInChildren<AudioSource>(true);

        foreach (var src in childSources)
        {
            // Ignore the SoundManager's own AudioSource if it has one
            if (src.gameObject == gameObject) continue;

            string soundName = src.gameObject.name;

            if (soundSources.ContainsKey(soundName))
            {
                Debug.LogWarning($"[SoundManager] Duplicate sound child object name '{soundName}' found.", src.gameObject);
                continue;
            }

            soundSources.Add(soundName, src);

            // Ensure they start disabled as requested
            src.gameObject.SetActive(false);
        }
    }

    // ------------------------------------------------------------------
    // PUBLIC API
    // ------------------------------------------------------------------

    /// <summary>
    /// Play a sound by name. Defaults to not looped. 
    /// Non-looped sounds will enable the child object, play, and auto-disable when finished.
    /// </summary>
    public void Play(string name, bool isLooped = false)
    {
        if (!TryGetAudioSource(name, out AudioSource source)) return;

        // Stop any pending auto-disable coroutine if it's already playing
        if (activeCoroutines.TryGetValue(name, out Coroutine routine) && routine != null)
        {
            StopCoroutine(routine);
            activeCoroutines.Remove(name);
        }

        GameObject soundObj = source.gameObject;
        soundObj.SetActive(true);

        source.loop = isLooped;
        source.volume = masterVolume; // You can multiply by individual volume if you add a per-object setting

        if (!source.isPlaying)
        {
            source.Play();
        }

        // If it's not looped, start a coroutine to disable it after the clip finishes
        if (!isLooped)
        {
            if (source.clip != null)
            {
                float duration = source.clip.length / Mathf.Max(source.pitch, 0.1f);
                activeCoroutines[name] = StartCoroutine(DisableAfterDuration(soundObj, duration, name));
            }
            else
            {
                Debug.LogWarning($"[SoundManager] Sound '{name}' has no AudioClip assigned.");
                soundObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Stops a sound and immediately disables its child object.
    /// </summary>
    public void Stop(string name)
    {
        if (!TryGetAudioSource(name, out AudioSource source)) return;

        if (activeCoroutines.TryGetValue(name, out Coroutine routine) && routine != null)
        {
            StopCoroutine(routine);
            activeCoroutines.Remove(name);
        }

        source.Stop();
        source.gameObject.SetActive(false);
    }

    /// <summary>
    /// Stops and disables all sound child objects.
    /// </summary>
    public void StopAll()
    {
        foreach (var kvp in activeCoroutines)
        {
            if (kvp.Value != null) StopCoroutine(kvp.Value);
        }
        activeCoroutines.Clear();

        foreach (var source in soundSources.Values)
        {
            if (source != null)
            {
                source.Stop();
                source.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Global master volume multiplier (0 to 1).
    /// </summary>
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        foreach (var source in soundSources.Values)
        {
            if (source != null)
            {
                source.volume = masterVolume;
            }
        }
    }

    public float MasterVolume => masterVolume;

    /// <summary>
    /// Returns true if the sound's child object is active and playing.
    /// </summary>
    public bool IsPlaying(string name)
    {
        if (soundSources.TryGetValue(name, out AudioSource source))
        {
            return source != null && source.isPlaying;
        }
        return false;
    }

    // ------------------------------------------------------------------
    // INTERNALS
    // ------------------------------------------------------------------

    private bool TryGetAudioSource(string name, out AudioSource source)
    {
        if (soundSources.TryGetValue(name, out source) && source != null)
        {
            return true;
        }

        Debug.LogWarning($"[SoundManager] Sound child object '{name}' not found.");
        source = null;
        return false;
    }

    private IEnumerator DisableAfterDuration(GameObject soundObj, float duration, string name)
    {
        yield return new WaitForSeconds(duration);

        if (soundObj != null)
        {
            // Make sure it didn't get restarted in the meantime
            if (soundSources.TryGetValue(name, out AudioSource src) && !src.isPlaying)
            {
                soundObj.SetActive(false);
            }
        }

        activeCoroutines.Remove(name);
    }
}