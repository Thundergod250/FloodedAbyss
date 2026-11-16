using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    // Dictionary mapping sound names to their corresponding AudioSources on child objects
    private Dictionary<string, AudioSource> soundSources = new();

    // Tracks active disable coroutines for non-looping sounds so they don't conflict
    private Dictionary<string, Coroutine> activeCoroutines = new();

    // Caches the default inspector volumes for each source so we can scale them relative to master volume
    private Dictionary<string, float> inspectorVolumes = new();

    private void Awake() => InitializeSoundSources();

    private void InitializeSoundSources()
    {
        soundSources.Clear();
        inspectorVolumes.Clear();
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
            inspectorVolumes.Add(soundName, src.volume); // Cache the volume set in the Inspector (e.g. 0.35)

            // 1. Preload the audio clip data into memory so it doesn't hitch on first play
            if (src.clip != null) 
                src.clip.LoadAudioData();

            // 2. Briefly activate and deactivate the GameObject to prewarm Unity's audio components and mixer routing
            GameObject soundObj = src.gameObject;
            bool wasActive = soundObj.activeSelf;

            soundObj.SetActive(true);
            soundObj.SetActive(false);
        }
    }
    
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
        
        // Multiplies the inspector volume (e.g. 0.35) by the master volume slider, preserving your custom tweaks!
        float baseVolume = inspectorVolumes.TryGetValue(name, out float vol) ? vol : 1f;
        source.volume = baseVolume * masterVolume;

        if (!source.isPlaying) 
            source.Play();

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
                soundObj.SetActive(false);
        }

        activeCoroutines.Remove(name);
    }
}