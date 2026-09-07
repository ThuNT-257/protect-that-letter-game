using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    #region Serialized Fields
    [Header("--- Audio Sources ---")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("--- Audio Clips ---")]
    [SerializeField] private List<AudioClip> bgmList = new List<AudioClip>();
    [SerializeField] private List<AudioClip> sfxList = new List<AudioClip>();
    #endregion

    #region Private Fields
    private int currentBgmIndex = 0;
    private bool isBgmPlayingRequested = false;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    #endregion

    #region Lifecycle
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXDictionary();
        } else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        if (isBgmPlayingRequested && bgmSource != null && !bgmSource.isPlaying && bgmList.Count > 0) {
            PlayNextBGM();
        }
    }
    #endregion

    #region Public BGM Methods
    public void PlayBGMIndex(int index) {
        if (bgmList == null || bgmList.Count == 0 || bgmSource == null) return;

        currentBgmIndex = index % bgmList.Count;
        AudioClip nextClip = bgmList[currentBgmIndex];

        if (bgmSource.isPlaying && bgmSource.clip == nextClip) {
            return;
        }

        bgmSource.clip = nextClip;
        bgmSource.Play();
        isBgmPlayingRequested = true;
    }

    public void PlayNextBGM() {
        if (bgmList == null || bgmList.Count == 0 || bgmSource == null) return;

        currentBgmIndex = (currentBgmIndex + 1) % bgmList.Count;
        bgmSource.clip = bgmList[currentBgmIndex];
        bgmSource.Play();
    }

    public void SetBGMMute(bool isMuted) {
        if (bgmSource != null) {
            bgmSource.mute = isMuted;
        }
    }
    #endregion

    #region Public SFX Methods
    public void PlaySFX(string clipName) {
        if (sfxSource == null || string.IsNullOrEmpty(clipName)) return;

        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip)) {
            sfxSource.PlayOneShot(clip);
        } else {
            Debug.LogWarning($"[AudioManager] SFX Clip '{clipName}' not found in sfxList!");
        }
    }

    public void PlaySFX(AudioClip clip) {
        if (sfxSource != null && clip != null) {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void SetSFXMute(bool isMuted) {
        if (sfxSource != null) {
            sfxSource.mute = isMuted;
        }
    }
    #endregion

    #region Private Methods
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