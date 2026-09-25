using System.Collections.Generic;
using UnityEngine;

namespace ProtectThatLetter.Managers {
    /// <summary>
    /// Centralized audio manager using a unified audio clip list and lookup dictionary.
    /// Manages BGM, SFX, looped SFX, and story audio.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour {
        #region Instance
        // Singleton instance with public getter and private setter
        public static AudioManager Instance { get; private set; }
        #endregion

        #region Serialized Fields
        [Header("--- Audio Sources ---")]
        [SerializeField] private AudioSource bgmSource;         // Background music source
        [SerializeField] private AudioSource sfxSource;         // One-shot SFX source
        [SerializeField] private AudioSource loopSfxSource;     // Looping SFX source
        [SerializeField] private AudioSource storyAudioSource;  // Story-specific audio source

        [Header("--- Shared Audio Library ---")]
        [SerializeField] private AudioClip defaultBgmClip;      // Default BGM track
        [SerializeField] private List<AudioClip> soundsList = new List<AudioClip>(); // Shared audio clip library
        #endregion

        #region Private Fields
        private bool isStoryMuted = false;
        // Centralized cache dictionary for fast lookup (O(1)) across all channels
        private readonly Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Ensures singleton integrity and builds the sound dictionary
        /// </summary>
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            InitializeSoundDictionary();
        }

        /// <summary>
        /// Subscribes to SettingsManager events when enabled
        /// </summary>
        private void OnEnable() {
            SettingsManager.OnBGMSettingChanged += HandleBGMChanged;
            SettingsManager.OnSFXSettingChanged += HandleSFXChanged;
        }

        /// <summary>
        /// Applies saved audio settings and starts BGM on startup
        /// </summary>
        private void Start() {
            if (SettingsManager.Instance != null) {
                SetBGMMute(!SettingsManager.Instance.IsBGMOn); // Invert: true = mute
                SetSFXMute(!SettingsManager.Instance.IsSFXOn); // Invert: true = mute
            }

            PlayBGM();
        }

        /// <summary>
        /// Unsubscribes from events to prevent memory leaks
        /// </summary>
        private void OnDisable() {
            SettingsManager.OnBGMSettingChanged -= HandleBGMChanged;
            SettingsManager.OnSFXSettingChanged -= HandleSFXChanged;
        }

        /// <summary>
        /// Clears the singleton reference when destroyed
        /// </summary>
        private void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }
        #endregion

        #region Public Methods
        // --- BGM ---

        /// <summary>
        /// Plays the default BGM clip
        /// </summary>
        public void PlayBGM() {
            if (defaultBgmClip != null) {
                PlayBGM(defaultBgmClip);
            }
        }

        /// <summary>
        /// Plays a BGM track by clip name from the shared sounds list or Resources
        /// </summary>
        public void PlayBGM(string clipName) {
            if (bgmSource == null || string.IsNullOrEmpty(clipName)) return;

            AudioClip clip = GetAudioClip(clipName);
            if (clip != null) {
                PlayBGM(clip);
            } else {
                Debug.LogWarning($"[AudioManager] BGM Clip '{clipName}' not found!");
            }
        }

        /// <summary>
        /// Plays a BGM track by AudioClip reference
        /// </summary>
        public void PlayBGM(AudioClip clip) {
            if (bgmSource == null || clip == null) return;

            // Skip if the same clip is already playing
            if (bgmSource.isPlaying && bgmSource.clip == clip) return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        /// <summary>
        /// Stops the BGM
        /// </summary>
        public void StopBGM() {
            if (bgmSource != null) {
                bgmSource.Stop();
            }
        }

        /// <summary>
        /// Sets the BGM mute state
        /// </summary>
        public void SetBGMMute(bool isMuted) {
            if (bgmSource != null) {
                bgmSource.mute = isMuted;
            }
        }

        // --- SFX ---

        /// <summary>
        /// Plays a one-shot SFX by clip name from the shared sounds list
        /// </summary>
        public void PlaySFX(string clipName) {
            if (sfxSource == null || string.IsNullOrEmpty(clipName)) return;

            AudioClip clip = GetAudioClip(clipName);
            if (clip != null) {
                sfxSource.PlayOneShot(clip);
            } else {
                Debug.LogWarning($"[AudioManager] SFX Clip '{clipName}' not found!");
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
        /// Plays a looping SFX by clip name
        /// </summary>
        public void PlayLoopSFX(string clipName) {
            AudioSource targetSource = loopSfxSource != null ? loopSfxSource : sfxSource;
            if (targetSource == null || string.IsNullOrEmpty(clipName)) return;

            AudioClip clip = GetAudioClip(clipName);
            if (clip != null) {
                targetSource.clip = clip;
                targetSource.loop = true;
                targetSource.Play();
            } else {
                Debug.LogWarning($"[AudioManager] Loop SFX Clip '{clipName}' not found!");
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

        // --- Story Audio ---

        /// <summary>
        /// Plays story audio by clip name from the shared sounds list or Resources
        /// </summary>
        public void PlayStorySFX(string clipName) {
            if (storyAudioSource == null || string.IsNullOrEmpty(clipName) || isStoryMuted) return;

            AudioClip clip = GetAudioClip(clipName);
            if (clip != null) {
                storyAudioSource.PlayOneShot(clip);
            } else {
                Debug.LogWarning($"[AudioManager] Story SFX Clip '{clipName}' not found!");
            }
        }

        /// <summary>
        /// Plays story audio by AudioClip reference
        /// </summary>
        public void PlayStorySFX(AudioClip clip) {
            if (storyAudioSource == null || clip == null || isStoryMuted) return;
            storyAudioSource.PlayOneShot(clip);
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Builds the initial lookup dictionary from serialized soundsList
        /// </summary>
        private void InitializeSoundDictionary() {
            soundDictionary.Clear();

            // Add default BGM to dictionary
            if (defaultBgmClip != null && !soundDictionary.ContainsKey(defaultBgmClip.name)) {
                soundDictionary.Add(defaultBgmClip.name, defaultBgmClip);
            }

            // Add all clips from soundsList
            foreach (var clip in soundsList) {
                if (clip != null && !soundDictionary.ContainsKey(clip.name)) {
                    soundDictionary.Add(clip.name, clip);
                }
            }
        }

        /// <summary>
        /// Helper to retrieve clip from cache or load dynamically from Resources/Sounds/
        /// </summary>
        private AudioClip GetAudioClip(string clipName) {
            if (string.IsNullOrEmpty(clipName)) return null;

            // 1. Check Dictionary cache
            if (soundDictionary.TryGetValue(clipName, out AudioClip cachedClip)) {
                return cachedClip;
            }

            // 2. Load from Resources and cache for future reuse
            AudioClip loadedClip = Resources.Load<AudioClip>($"Sounds/{clipName}");
            if (loadedClip != null) {
                soundDictionary.Add(clipName, loadedClip);
                return loadedClip;
            }

            return null;
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
    }
}