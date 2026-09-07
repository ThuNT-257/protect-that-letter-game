using UnityEngine;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Adjusts dialogue frame anchors and character avatar layouts dynamically based on screen orientation (Landscape / Portrait).
    /// Preserves default Inspector layout when in Landscape mode.
    /// </summary>
    [DisallowMultipleComponent]
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

        private AnchorData defaultFrameAnchors;
        private AnchorData defaultLeftAvatarAnchors;
        private AnchorData defaultRightAvatarAnchors;
        #endregion

        #region Data Structures
        private struct AnchorData {
            public Vector2 min;
            public Vector2 max;
            public Vector2 pivot;
            public Vector2 offsetMin;
            public Vector2 offsetMax;

            public AnchorData(RectTransform rect) {
                min = rect.anchorMin;
                max = rect.anchorMax;
                pivot = rect.pivot;
                offsetMin = rect.offsetMin;
                offsetMax = rect.offsetMax;
            }
        }
        #endregion

        #region Lifecycle
        private void Awake() {
            if (dialogueLineFrame != null) defaultFrameAnchors = new AnchorData(dialogueLineFrame);
            if (leftAvatar != null) defaultLeftAvatarAnchors = new AnchorData(leftAvatar);
            if (rightAvatar != null) defaultRightAvatarAnchors = new AnchorData(rightAvatar);
        }

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
                RestoreAnchors(dialogueLineFrame, defaultFrameAnchors);
            } else {
                dialogueLineFrame.anchorMin = new Vector2(0.05f, 0.05f);
                dialogueLineFrame.anchorMax = new Vector2(0.95f, 0.25f);
                dialogueLineFrame.offsetMin = Vector2.zero;
                dialogueLineFrame.offsetMax = Vector2.zero;
            }
        }

        private void ApplyAvatarVisibilityAndPosition() {
            if (isLandscape) {
                RestoreAnchors(leftAvatar, defaultLeftAvatarAnchors);
                RestoreAnchors(rightAvatar, defaultRightAvatarAnchors);

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

        private void RestoreAnchors(RectTransform rect, AnchorData data) {
            if (rect == null) return;

            rect.anchorMin = data.min;
            rect.anchorMax = data.max;
            rect.pivot = data.pivot;
            rect.offsetMin = data.offsetMin;
            rect.offsetMax = data.offsetMax;
        }
        #endregion
    }
}