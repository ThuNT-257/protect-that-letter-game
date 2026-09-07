using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the visual presentation of dialogue including characters, backgrounds, and text
/// </summary>
public class DialogueUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Background Elements")]
    [SerializeField] private Image backgroundImage;

    [Header("Audio Elements")]
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Left Character Elements")]
    [SerializeField] private Image leftNameFrame;
    [SerializeField] private TMP_Text leftNameText;
    [SerializeField] private Image leftAvatarImage;

    [Header("Right Character Elements")]
    [SerializeField] private Image rightNameFrame;
    [SerializeField] private TMP_Text rightNameText;
    [SerializeField] private Image rightAvatarImage;

    [Header("Dialogue Elements")]
    [SerializeField] private TMP_Text dialogueLineText;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button nextButton;
    #endregion

    #region Events
    public static event Action OnNextButtonClicked;
    #endregion

    #region Lifecycle
    private void Awake() {
        if(skipButton != null) {
            skipButton.onClick.AddListener(() => OnNextButtonClicked?.Invoke());
        }
        if(nextButton != null) {
            nextButton.onClick.AddListener(() => OnNextButtonClicked?.Invoke());
        }
    }
    #endregion

    //position = 0 (begin dialogue), 1 (end dialogue), -1 (normal)
    #region Public Methods
    /// <summary>
    /// Displays a dialogue line with background, sound, character, and text
    /// </summary>
    /// <param name="line">The dialogue line to display</param>
    public void DisplayLine(DialogueLine line) {
        if(line == null) {
            return;
        }

        //background image
        if(!string.IsNullOrEmpty(line.background) && backgroundImage != null) {
            Sprite backgroundSprite = Resources.Load<Sprite>($"Backgrounds/{line.background}");
            if(backgroundSprite != null) {
                backgroundImage.sprite = backgroundSprite;
            }
        }

        //sound effects
        if(!string.IsNullOrEmpty(line.sound) && sfxAudioSource != null) {
            AudioClip clip = Resources.Load<AudioClip>($"Sounds/{line.sound}");
            if(clip != null) {
                sfxAudioSource.PlayOneShot(clip);
            }
        }

        //dialogue text
        if(dialogueLineText != null) {
            dialogueLineText.text = line.content;
        }

        //left/right character
        bool isLeft = string.IsNullOrEmpty(line.position) || line.position.ToLower() == "left";
        if (isLeft) {
            SetCharacterUI(leftNameFrame, leftNameText, leftAvatarImage, line);
            SetInactiveUI(rightNameFrame, rightAvatarImage);
        } else {
            SetCharacterUI(rightNameFrame, rightNameText, rightAvatarImage, line);
            SetInactiveUI(leftNameFrame, leftAvatarImage);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Activates and sets up the active character's UI elements
    /// </summary>
    private void SetCharacterUI(Image nameFrame, TMP_Text nameText, Image avatarImage, DialogueLine line) {
        if(nameFrame != null) {
            nameFrame.gameObject.SetActive(true);
        }

        if (nameText != null) {
            nameText.text = line.speaker;
        }

        if(avatarImage != null) {
            if (!string.IsNullOrEmpty(line.avatar)) {
                avatarImage.gameObject.SetActive(true);
                avatarImage.color = Color.white;
                avatarImage.transform.localScale = Vector3.one;

                Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{line.avatar}");
                if (avatarSprite != null) {
                    avatarImage.sprite = avatarSprite;
                } else {
                    Debug.LogError($"[DialogueUI] Can not load: Resources/Avatars/{line.avatar}.");
                }
            }
        } else {
            Debug.LogError("[DialogueUI] Image is NULL");
        }
    }

    /// <summary>
    /// Deactivates or dims the inactive character's UI elements
    /// </summary>
    private void SetInactiveUI(Image nameFrame, Image avatarImage) {
        if(nameFrame != null) {
            nameFrame.gameObject.SetActive(false);
        }

        if (avatarImage != null && avatarImage.gameObject.activeSelf) {
            avatarImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            avatarImage.transform.localScale = transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
    }
    #endregion
}
