using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        [Tooltip("Unique ID used by other scripts to play this sound.")]
        public string name;

        public AudioClip clip;

        [Tooltip("Volume for this specific sound.")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Loop continuously until Stop() is called.")]
        public bool loop = false;

        [Tooltip("Pitch variation for variety (1 = normal).")]
        [Range(0.1f, 3f)]
        public float pitch = 1f;

        // Runtime-only: dedicated source for looping sounds
        [HideInInspector] public AudioSource loopSource;
    }

    [Header("Sound Library")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;

    [Tooltip("Reusable AudioSources for non-looping SFX.")]
    [SerializeField, Min(1)] private int oneShotSourceCount = 8;

    private Dictionary<string, SoundEntry> lookup;
    private AudioSource[] oneShotSources;
    private int nextOneShotIndex = 0;

    // ------------------------------------------------------------------
    // LIFECYCLE
    // ------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildLookup();
        BuildOneShotPool();
        BuildLoopSources();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, SoundEntry>(sounds.Count);
        foreach (var s in sounds)
        {
            if (string.IsNullOrEmpty(s.name))
            {
                Debug.LogWarning("[SoundManager] Sound entry has no name; skipped.");
                continue;
            }
            if (s.clip == null)
            {
                Debug.LogWarning($"[SoundManager] '{s.name}' has no clip assigned.");
            }
            if (lookup.ContainsKey(s.name))
            {
                Debug.LogWarning($"[SoundManager] Duplicate sound name '{s.name}'; using first.");
                continue;
            }
            lookup[s.name] = s;
        }
    }

    private void BuildOneShotPool()
    {
        oneShotSources = new AudioSource[oneShotSourceCount];
        for (int i = 0; i < oneShotSourceCount; i++)
        {
            var go = new GameObject($"OneShot_{i}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            oneShotSources[i] = src;
        }
    }

    private void BuildLoopSources()
    {
        foreach (var s in sounds)
        {
            if (!s.loop || s.clip == null) continue;

            var go = new GameObject($"Loop_{s.name}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.clip = s.clip;
            src.volume = s.volume * masterVolume;
            src.pitch = s.pitch;
            src.spatialBlend = 0f;
            s.loopSource = src;
        }
    }

    // ------------------------------------------------------------------
    // PUBLIC API
    // ------------------------------------------------------------------

    /// <summary>Play a sound by name. Respects the per-sound loop setting.</summary>
    public void Play(string name)
    {
        if (!TryGet(name, out var entry)) return;
        if (entry.clip == null) return;

        if (entry.loop)
        {
            if (entry.loopSource == null) return;
            entry.loopSource.volume = entry.volume * masterVolume;
            entry.loopSource.pitch = entry.pitch;
            if (!entry.loopSource.isPlaying) entry.loopSource.Play();
        }
        else
        {
            var src = GetNextOneShotSource();
            src.clip = entry.clip;
            src.pitch = entry.pitch;
            src.volume = entry.volume * masterVolume;
            src.Play();
        }
    }

    /// <summary>Stop a sound by name (works for loops and one-shots on the pool).</summary>
    public void Stop(string name)
    {
        if (!TryGet(name, out var entry)) return;

        if (entry.loopSource != null)
            entry.loopSource.Stop();

        foreach (var src in oneShotSources)
            if (src.clip == entry.clip) src.Stop();
    }

    /// <summary>Stop everything.</summary>
    public void StopAll()
    {
        foreach (var s in sounds)
            if (s.loopSource != null) s.loopSource.Stop();
        foreach (var src in oneShotSources) src.Stop();
    }

    /// <summary>Change a sound's volume at runtime.</summary>
    public void SetVolume(string name, float volume)
    {
        if (!TryGet(name, out var entry)) return;
        entry.volume = Mathf.Clamp01(volume);
        if (entry.loopSource != null)
            entry.loopSource.volume = entry.volume * masterVolume;
    }

    /// <summary>Toggle loop at runtime (takes effect next Play).</summary>
    public void SetLoop(string name, bool loop)
    {
        if (!TryGet(name, out var entry)) return;
        if (entry.loop == loop) return;

        entry.loop = loop;

        // Turning loop OFF while playing → stop the loop source
        if (!loop && entry.loopSource != null)
            entry.loopSource.Stop();

        // Turning loop ON at runtime → create a source if needed
        if (loop && entry.loopSource == null && entry.clip != null)
        {
            var go = new GameObject($"Loop_{entry.name}");
            go.transform.SetParent(transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.clip = entry.clip;
            src.volume = entry.volume * masterVolume;
            src.pitch = entry.pitch;
            src.spatialBlend = 0f;
            entry.loopSource = src;
        }
    }

    /// <summary>Global volume multiplier (0-1).</summary>
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        foreach (var s in sounds)
            if (s.loopSource != null)
                s.loopSource.volume = s.volume * masterVolume;
    }

    public float MasterVolume => masterVolume;

    /// <summary>True if this sound is currently playing.</summary>
    public bool IsPlaying(string name)
    {
        if (!TryGet(name, out var entry)) return false;

        if (entry.loopSource != null && entry.loopSource.isPlaying) return true;

        foreach (var src in oneShotSources)
            if (src.isPlaying && src.clip == entry.clip) return true;

        return false;
    }

    // ------------------------------------------------------------------
    // INTERNALS
    // ------------------------------------------------------------------
    private bool TryGet(string name, out SoundEntry entry)
    {
        if (!string.IsNullOrEmpty(name) && lookup != null && lookup.TryGetValue(name, out entry))
            return true;

        Debug.LogWarning($"[SoundManager] Sound '{name}' not found in library.");
        entry = null;
        return false;
    }

    private AudioSource GetNextOneShotSource()
    {
        // Prefer an idle source so we don't cut off a playing sound
        foreach (var src in oneShotSources)
            if (!src.isPlaying) return src;

        // All busy → round-robin (oldest gets recycled)
        var chosen = oneShotSources[nextOneShotIndex];
        nextOneShotIndex = (nextOneShotIndex + 1) % oneShotSources.Length;
        return chosen;
    }
}