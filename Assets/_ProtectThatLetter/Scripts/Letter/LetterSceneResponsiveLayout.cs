using UnityEngine;

namespace ProtectThatLetter.UI {
    [ExecuteAlways]
    public class LetterSceneResponsiveLayout : MonoBehaviour {
        #region Serialized Fields
        [Header("Letter Scene Containers")]
        [SerializeField] private RectTransform countdownContainer;
        [SerializeField] private RectTransform pagesContainer;
        [SerializeField] private RectTransform birdContainer;

        [Header("Base Portrait Settings")]
        [SerializeField] private Vector2 portraitCountdownAnchorMin = new Vector2(0.5f, 1f);
        [SerializeField] private Vector2 portraitCountdownAnchorMax = new Vector2(0.5f, 1f);
        [SerializeField] private Vector2 portraitCountdownAnchoredPos = new Vector2(0f, -75f);

        [SerializeField] private Vector3 portraitPagesScale = new Vector3(1.15f, 1.15f, 1f);
        [SerializeField] private Vector2 portraitPagesAnchoredPos = new Vector2(0f, 60f);

        [SerializeField] private Vector2 portraitBirdAnchorMin = new Vector2(0.5f, 0f);
        [SerializeField] private Vector2 portraitBirdAnchorMax = new Vector2(0.5f, 0f);
        [SerializeField] private Vector2 portraitBirdAnchoredPos = new Vector2(254f, 327f);

        [Header("Square-ish Portrait Adjustments (e.g. 3:4, iPad)")]
        [SerializeField] private Vector3 squarishPagesScale = new Vector3(0.85f, 0.85f, 1f);
        [SerializeField] private Vector2 squarishPagesAnchoredPos = new Vector2(0f, 20f);

        [Header("Landscape Presets")]
        [SerializeField] private Vector2 landscapeCountdownAnchorMin = new Vector2(0f, 0.5f);
        [SerializeField] private Vector2 landscapeCountdownAnchorMax = new Vector2(0f, 0.5f);
        [SerializeField] private Vector2 landscapeCountdownAnchoredPos = new Vector2(240f, 0f);

        [SerializeField] private Vector3 landscapePagesScale = Vector3.one;
        [SerializeField] private Vector2 landscapePagesAnchoredPos = Vector2.zero;

        [SerializeField] private Vector2 landscapeBirdAnchorMin = new Vector2(1f, 0f);
        [SerializeField] private Vector2 landscapeBirdAnchorMax = new Vector2(1f, 0f);
        [SerializeField] private Vector2 landscapeBirdAnchoredPos = new Vector2(-285f, 323f);
        #endregion

        #region Private Fields
        private bool isPortrait;
        private int lastWidth;
        private int lastHeight;
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
        public void UpdateLayout() {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            isPortrait = Screen.height > Screen.width;

            if (isPortrait) {
                ApplyPortraitLayout();
            } else {
                ApplyLandscapeLayout();
            }
        }
        #endregion

        #region Private Methods
        private void ApplyPortraitLayout() {
            float aspectRatio = (float)Screen.width / Screen.height; // 9/16 = 0.5625, 3/4 = 0.75

            float t = Mathf.InverseLerp(0.5625f, 0.75f, aspectRatio);

            Vector3 targetPagesScale = Vector3.Lerp(portraitPagesScale, squarishPagesScale, t);
            Vector2 targetPagesPos = Vector2.Lerp(portraitPagesAnchoredPos, squarishPagesAnchoredPos, t);

            ApplyTransform(countdownContainer, portraitCountdownAnchorMin, portraitCountdownAnchorMax, portraitCountdownAnchoredPos, Vector3.one);
            ApplyTransform(pagesContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), targetPagesPos, targetPagesScale);
            ApplyTransform(birdContainer, portraitBirdAnchorMin, portraitBirdAnchorMax, portraitBirdAnchoredPos, Vector3.one);
        }

        private void ApplyLandscapeLayout() {
            ApplyTransform(countdownContainer, landscapeCountdownAnchorMin, landscapeCountdownAnchorMax, landscapeCountdownAnchoredPos, Vector3.one);
            ApplyTransform(pagesContainer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), landscapePagesAnchoredPos, landscapePagesScale);
            ApplyTransform(birdContainer, landscapeBirdAnchorMin, landscapeBirdAnchorMax, landscapeBirdAnchoredPos, Vector3.one);
        }

        private void ApplyTransform(RectTransform target, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector3 scale) {
            if (target == null) return;
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.anchoredPosition = anchoredPos;
            target.localScale = scale;
        }
        #endregion
    }
}