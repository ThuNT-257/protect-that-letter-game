using ProtectThatLetter.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized audio manager for BGM, SFX, looped SFX, and story audio.
/// Reacts to SettingsManager events and persists across scenes.
/// </summary>
public class AudioManager : MonoBehaviour {
    #region Instance
    // Singleton instance with public getter and private setter
    public static AudioManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("--- Audio Sources ---")]
    [SerializeField] private AudioSource bgmSource;        // Background music source
    [SerializeField] private AudioSource sfxSource;        // One-shot SFX source
    [SerializeField] private AudioSource loopSfxSource;    // Looping SFX source
    [SerializeField] private AudioSource storyAudioSource; // Story-specific audio source

    [Header("--- Audio Clips ---")]
    [SerializeField] private List<AudioClip> bgmList = new List<AudioClip>(); // BGM playlist
    [SerializeField] private List<AudioClip> sfxList = new List<AudioClip>(); // SFX library
    #endregion

    #region Private Fields
    private int currentBgmIndex = 0;                            // Current BGM index
    private bool isBgmPlayingRequested = false;                 // True when BGM should be playing
    private bool isStoryMuted = false;                          // Story audio mute state
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>(); // Fast SFX lookup
    #endregion

    #region Lifecycle
    /// <summary>
    /// Ensures singleton integrity and initializes the SFX dictionary
    /// </summary>
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            InitializeSFXDictionary();
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Subscribes to SettingsManager events when enabled
    /// </summary>
    private void OnEnable() {
        SettingsManager.OnBGMSettingChanged += HandleBGMChanged;
        SettingsManager.OnSFXSettingChanged += HandleSFXChanged;
    }

    /// <summary>
    /// Applies saved audio settings on startup
    /// </summary>
    private void Start() {
        if (SettingsManager.Instance != null) {
            SetBGMMute(!SettingsManager.Instance.IsBGMOn); // Invert: true = mute
            SetSFXMute(!SettingsManager.Instance.IsSFXOn); // Invert: true = mute
        }
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks
    /// </summary>
    private void OnDisable() {
        SettingsManager.OnBGMSettingChanged -= HandleBGMChanged;
        SettingsManager.OnSFXSettingChanged -= HandleSFXChanged;
    }

    /// <summary>
    /// Auto-plays the next BGM track when the current one finishes
    /// </summary>
    private void Update() {
        if (isBgmPlayingRequested && bgmSource != null && !bgmSource.isPlaying && bgmList.Count > 0) {
            PlayNextBGM();
        }
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Reacts to BGM setting changes from SettingsManager
    /// </summary>
    private void HandleBGMChanged(bool isBGMOn) {
        SetBGMMute(!isBGMOn); // Invert: mute when OFF
    }

    /// <summary>
    /// Reacts to SFX setting changes from SettingsManager
    /// </summary>
    private void HandleSFXChanged(bool isSFXOn) {
        SetSFXMute(!isSFXOn); // Invert: mute when OFF
    }
    #endregion

    #region Public BGM Methods
    /// <summary>
    /// Plays BGM at the specified index (wraps around)
    /// </summary>
    public void PlayBGMIndex(int index) {
        if (bgmList == null || bgmList.Count == 0 || bgmSource == null) return;

        currentBgmIndex = index % bgmList.Count;
        AudioClip nextClip = bgmList[currentBgmIndex];

        // Skip if the same track is already playing
        if (bgmSource.isPlaying && bgmSource.clip == nextClip) return;

        bgmSource.clip = nextClip;
        bgmSource.Play();
        isBgmPlayingRequested = true;
    }

    /// <summary>
    /// Advances to the next BGM track in the playlist
    /// </summary>
    public void PlayNextBGM() {
        if (bgmList == null || bgmList.Count == 0 || bgmSource == null) return;

        currentBgmIndex = (currentBgmIndex + 1) % bgmList.Count;
        bgmSource.clip = bgmList[currentBgmIndex];
        bgmSource.Play();
    }

    /// <summary>
    /// Sets the BGM mute state
    /// </summary>
    public void SetBGMMute(bool isMuted) {
        if (bgmSource != null) {
            bgmSource.mute = isMuted;
        }
    }
    #endregion

    #region Public SFX Methods
    /// <summary>
    /// Plays a one-shot SFX by clip name
    /// </summary>
    public void PlaySFX(string clipName) {
        if (sfxSource == null || string.IsNullOrEmpty(clipName)) return;

        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip)) {
            sfxSource.PlayOneShot(clip);
        } else {
            Debug.LogWarning($"[AudioManager] SFX Clip '{clipName}' not found in sfxList!");
        }
    }

    /// <summary>
    /// Plays a one-shot SFX by AudioClip reference
    /// </summary>
    public void PlaySFX(AudioClip clip) {
        if (sfxSource != null && clip != null) {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Plays a looping SFX (uses loopSfxSource if available, otherwise sfxSource)
    /// </summary>
    public void PlayLoopSFX(string clipName) {
        AudioSource targetSource = loopSfxSource != null ? loopSfxSource : sfxSource;
        if (targetSource == null || string.IsNullOrEmpty(clipName)) return;

        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip)) {
            targetSource.clip = clip;
            targetSource.loop = true;
            targetSource.Play();
        } else {
            Debug.LogWarning($"[AudioManager] Loop SFX Clip '{clipName}' not found in sfxList!");
        }
    }

    /// <summary>
    /// Stops the currently playing looping SFX
    /// </summary>
    public void StopLoopSFX() {
        AudioSource targetSource = loopSfxSource != null ? loopSfxSource : sfxSource;
        if (targetSource != null) {
            targetSource.Stop();
            targetSource.loop = false;
        }
    }

    /// <summary>
    /// Sets the SFX mute state (both one-shot and looped)
    /// </summary>
    public void SetSFXMute(bool isMuted) {
        if (sfxSource != null) sfxSource.mute = isMuted;
        if (loopSfxSource != null) loopSfxSource.mute = isMuted;
    }
    #endregion

    #region Public Story Audio Methods
    /// <summary>
    /// Plays story SFX by AudioClip reference (respects story mute)
    /// </summary>
    public void PlayStorySFX(AudioClip clip) {
        if (storyAudioSource == null || clip == null || isStoryMuted) return;
        storyAudioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Plays story SFX by clip name (falls back to Resources/Sounds/)
    /// </summary>
    public void PlayStorySFX(string clipName) {
        if (storyAudioSource == null || string.IsNullOrEmpty(clipName) || isStoryMuted) return;

        // Try SFX dictionary first
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip)) {
            storyAudioSource.PlayOneShot(clip);
        } else {
            // Fallback: load from Resources
            AudioClip loadedClip = Resources.Load<AudioClip>($"Sounds/{clipName}");
            if (loadedClip != null) {
                storyAudioSource.PlayOneShot(loadedClip);
            } else {
                Debug.LogWarning($"[AudioManager] Story SFX Clip '{clipName}' not found!");
            }
        }
    }

    /// <summary>
    /// Sets the story audio mute state
    /// </summary>
    public void SetStoryMute(bool isMuted) {
        isStoryMuted = isMuted;
        if (storyAudioSource != null) {
            storyAudioSource.mute = isMuted;
        }
    }

    /// <summary>
    /// Stops all SFX (one-shot and looped)
    /// </summary>
    public void StopSFX() {
        if (sfxSource != null) {
            sfxSource.Stop();
        }
        if (loopSfxSource != null) {
            loopSfxSource.Stop();
            loopSfxSource.loop = false;
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Builds the SFX dictionary for fast clip lookup by name
    /// </summary>
    private void InitializeSFXDictionary() {
        sfxDictionary.Clear();
        foreach (var clip in sfxList) {
            if (clip != null && !sfxDictionary.ContainsKey(clip.name)) {
                sfxDictionary.Add(clip.name, clip);
            }
        }
    }
    #endregion
}