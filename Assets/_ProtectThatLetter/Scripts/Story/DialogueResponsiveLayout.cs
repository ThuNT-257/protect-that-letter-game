using UnityEngine;

public class DialogueResponsiveLayout : MonoBehaviour {
    #region Serialized Fields
    [Header("UI Elements")]
    [SerializeField] private RectTransform dialogueLineFrame;
    [SerializeField] private RectTransform leftAvatar;
    [SerializeField] private RectTransform rightAvatar;

    [Header("Settings")]
    [SerializeField] private float landscapeThreshold = 1.1f;
    #endregion

    #region Private Fields
    private bool isLandscape;
    private int lastWidth;
    private int lastHeight;
    private bool isLeftSpeaking = true;
    #endregion

    #region Lifecycle
    private void Start() {
        UpdateLayout();
    }

    private void Update() {
        if (Screen.width != lastWidth || Screen.height != lastHeight) {
            UpdateLayout();
        }
    }
    #endregion

    #region Public Methods
    public void SetSpeaker(bool isLeft) {
        isLeftSpeaking = isLeft;
        ApplyAvatarVisibilityAndPosition();
    }
    #endregion

    #region Private Methods
    private void UpdateLayout() {
        lastWidth = Screen.width;
        lastHeight = Screen.height;

        float aspectRatio = (float)lastWidth / lastHeight;
        isLandscape = aspectRatio > landscapeThreshold;

        ApplyFrameAnchors();
        ApplyAvatarVisibilityAndPosition();
    }

    private void ApplyFrameAnchors() {
        if (dialogueLineFrame == null) return;

        if (isLandscape) {
            dialogueLineFrame.anchorMin = new Vector2(0.05f, 0.05f);
            dialogueLineFrame.anchorMax = new Vector2(0.95f, 0.35f);
        } else {
            dialogueLineFrame.anchorMin = new Vector2(0.05f, 0.05f);
            dialogueLineFrame.anchorMax = new Vector2(0.95f, 0.25f);
        }

        dialogueLineFrame.offsetMin = Vector2.zero;
        dialogueLineFrame.offsetMax = Vector2.zero;
    }

    private void ApplyAvatarVisibilityAndPosition() {
        if (isLandscape) {
            SetAvatarTransform(leftAvatar, new Vector2(0f, 0f), new Vector2(0.35f, 0.5f), new Vector2(0f, 0f));
            SetAvatarTransform(rightAvatar, new Vector2(0.65f, 0f), new Vector2(1f, 0.5f), new Vector2(1f, 0f));

            if (leftAvatar != null) leftAvatar.gameObject.SetActive(true);
            if (rightAvatar != null) rightAvatar.gameObject.SetActive(true);
        } else {
            SetAvatarTransform(leftAvatar, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.65f), new Vector2(0.5f, 0f));
            SetAvatarTransform(rightAvatar, new Vector2(0.15f, 0.25f), new Vector2(0.85f, 0.65f), new Vector2(0.5f, 0f));

            if (leftAvatar != null) leftAvatar.gameObject.SetActive(isLeftSpeaking);
            if (rightAvatar != null) rightAvatar.gameObject.SetActive(!isLeftSpeaking);
        }
    }

    private void SetAvatarTransform(RectTransform avatar, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot) {
        if (avatar == null) return;

        avatar.anchorMin = anchorMin;
        avatar.anchorMax = anchorMax;
        avatar.pivot = pivot;

        avatar.offsetMin = Vector2.zero;
        avatar.offsetMax = Vector2.zero;
    }
    #endregion
}