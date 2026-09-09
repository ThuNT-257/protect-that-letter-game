using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    [DisallowMultipleComponent]
    public class DialogueResponsiveLayout : MonoBehaviour {
        #region Serialized Fields
        [Header("UI Elements")]
        [SerializeField] private RectTransform dialogueLineFrame;
        [SerializeField] private RectTransform leftAvatar;
        [SerializeField] private RectTransform rightAvatar;

        [Header("Settings")]
        [SerializeField] private float landscapeThreshold = 1.1f;
        [SerializeField] private float avatarAnimDuration = 0.3f;
        [SerializeField] private float inactiveScale = 0.75f;
        [SerializeField] private float inactiveShiftX = 50f;
        #endregion

        #region Public Properties
        public bool IsLandscape => isLandscape;
        #endregion

        #region Private Fields
        private bool isLandscape;
        private int lastWidth;
        private int lastHeight;
        private bool isLeftSpeaking = true;
        private bool isOverlayHidden = true;

        private AnchorData defaultFrameAnchors;
        private AnchorData defaultLeftAvatarAnchors;
        private AnchorData defaultRightAvatarAnchors;

        private Coroutine leftAvatarCoroutine;
        private Coroutine rightAvatarCoroutine;
        #endregion

        #region Data Structures
        private struct AnchorData {
            public Vector2 min;
            public Vector2 max;
            public Vector2 pivot;
            public Vector2 offsetMin;
            public Vector2 offsetMax;
            public Vector2 anchoredPos;

            public AnchorData(RectTransform rect) {
                min = rect.anchorMin;
                max = rect.anchorMax;
                pivot = rect.pivot;
                offsetMin = rect.offsetMin;
                offsetMax = rect.offsetMax;
                anchoredPos = rect.anchoredPosition;
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

        public void SetOverlayHidden(bool isHidden) {
            isOverlayHidden = isHidden;
            Debug.Log($"[DialogueDebug] SetOverlayHidden -> isHidden: {isHidden}");
            if (!isOverlayHidden) {
                ApplyFrameAnchors();
                ApplyAvatarVisibilityAndPosition();
            }
        }

        public void AnimateLandscapeAvatar(bool isLeft, bool isSpeaking, bool isFirstLine) {
            string sideStr = isLeft ? "LEFT" : "RIGHT";
            Debug.Log($"[DialogueDebug] AnimateLandscapeAvatar -> Target: {sideStr} | IsSpeaking: {isSpeaking} | IsFirstLine: {isFirstLine} | IsLandscape: {isLandscape} | IsOverlayHidden: {isOverlayHidden}");

            if (!isLandscape || isOverlayHidden) {
                Debug.LogWarning($"[DialogueDebug] AnimateLandscapeAvatar Skipped -> IsLandscape: {isLandscape}, IsOverlayHidden: {isOverlayHidden}");
                return;
            }

            RectTransform targetAvatar = isLeft ? leftAvatar : rightAvatar;
            AnchorData defaultData = isLeft ? defaultLeftAvatarAnchors : defaultRightAvatarAnchors;

            if (targetAvatar == null) {
                Debug.LogError($"[DialogueDebug] targetAvatar is NULL for side: {sideStr}");
                return;
            }

            if (isFirstLine && !isSpeaking) {
                targetAvatar.gameObject.SetActive(false);
                Debug.Log($"<color=cyan>[DialogueDebug] FIRST LINE & INACTIVE -> Successfully Deactivated Avatar: {sideStr}</color>");
                return;
            }

            targetAvatar.gameObject.SetActive(true);
            Debug.Log($"<color=green>[DialogueDebug] Activated Avatar: {sideStr}</color>");

            RestoreAnchors(targetAvatar, defaultData);

            Vector3 targetScale = isSpeaking ? Vector3.one : new Vector3(inactiveScale, inactiveScale, 1f);

            Vector2 targetPosition = defaultData.anchoredPos;

            if (!isSpeaking) {
                float shiftX = isLeft ? -inactiveShiftX : inactiveShiftX;
                targetPosition.x += shiftX;

                float height = targetAvatar.rect.height;
                float scaleDifference = 1f - inactiveScale;

                float bottomOffset = height * scaleDifference * defaultData.pivot.y;
                targetPosition.y -= bottomOffset;
            }

            Image img = targetAvatar.GetComponent<Image>();
            Color targetColor = isSpeaking ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);

            if (isLeft) {
                if (leftAvatarCoroutine != null) StopCoroutine(leftAvatarCoroutine);
                leftAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(targetAvatar, img, targetScale, targetPosition, targetColor, isSpeaking));
            } else {
                if (rightAvatarCoroutine != null) StopCoroutine(rightAvatarCoroutine);
                rightAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(targetAvatar, img, targetScale, targetPosition, targetColor, isSpeaking));
            }
        }
        #endregion

        #region Private Methods
        private void UpdateLayout() {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            float aspectRatio = (float)lastWidth / lastHeight;
            isLandscape = aspectRatio > landscapeThreshold;

            Debug.Log($"[DialogueDebug] UpdateLayout -> Screen: {lastWidth}x{lastHeight} | IsLandscape: {isLandscape}");

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
            Debug.Log($"[DialogueDebug] ApplyAvatarVisibilityAndPosition -> IsOverlayHidden: {isOverlayHidden} | IsLandscape: {isLandscape}");
            if (isOverlayHidden) return;

            if (!isLandscape) {
                if (leftAvatar != null) {
                    SetupPortraitAvatar(leftAvatar, isLeft: true);
                    leftAvatar.gameObject.SetActive(isLeftSpeaking);
                    Debug.Log($"[DialogueDebug] Portrait Apply -> Left Avatar SetActive: {isLeftSpeaking}");
                }

                if (rightAvatar != null) {
                    SetupPortraitAvatar(rightAvatar, isLeft: false);
                    rightAvatar.gameObject.SetActive(!isLeftSpeaking);
                    Debug.Log($"[DialogueDebug] Portrait Apply -> Right Avatar SetActive: {!isLeftSpeaking}");
                }
            }
        }

        private void SetupPortraitAvatar(RectTransform avatar, bool isLeft) {
            if (avatar == null) return;

            Image img = avatar.GetComponent<Image>();
            if (img == null) return;

            img.preserveAspect = false;
            avatar.anchorMin = new Vector2(0f, 0f);
            avatar.anchorMax = new Vector2(1f, 0f);
            avatar.pivot = new Vector2(isLeft ? 0f : 1f, 0f);
            avatar.offsetMin = new Vector2(0f, avatar.offsetMin.y);
            avatar.offsetMax = new Vector2(0f, avatar.offsetMax.y);
            avatar.anchoredPosition = Vector2.zero;
            avatar.localScale = Vector3.one;

            if (img.sprite != null) {
                AspectRatioFitter fitter = avatar.GetComponent<AspectRatioFitter>();
                if (fitter == null) fitter = avatar.gameObject.AddComponent<AspectRatioFitter>();

                fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                fitter.aspectRatio = img.sprite.rect.width / img.sprite.rect.height;
            }

            Canvas.ForceUpdateCanvases();
        }

        private void RestoreAnchors(RectTransform rect, AnchorData data) {
            if (rect == null) return;

            AspectRatioFitter fitter = rect.GetComponent<AspectRatioFitter>();
            if (fitter != null) Destroy(fitter);

            rect.anchorMin = data.min;
            rect.anchorMax = data.max;
            rect.pivot = data.pivot;
            rect.offsetMin = data.offsetMin;
            rect.offsetMax = data.offsetMax;
        }

        private IEnumerator AnimateAvatarRoutine(RectTransform avatar, Image img, Vector3 targetScale, Vector2 targetPos, Color targetColor, bool isSpeaking) {
            float elapsed = 0f;
            Vector3 startScale = avatar.localScale;
            Vector2 startPos = avatar.anchoredPosition;
            Color startColor = img != null ? img.color : Color.white;

            while (elapsed < avatarAnimDuration) {
                elapsed += Time.deltaTime;
                float t = elapsed / avatarAnimDuration;

                float scaleT = isSpeaking ? EaseOutBack(t) : EaseOutCubic(t);

                avatar.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleT);
                avatar.anchoredPosition = Vector2.Lerp(startPos, targetPos, EaseOutCubic(t));

                if (img != null) {
                    img.color = Color.Lerp(startColor, targetColor, t);
                }

                yield return null;
            }

            avatar.localScale = targetScale;
            avatar.anchoredPosition = targetPos;
            if (img != null) img.color = targetColor;
        }

        private float EaseOutBack(float x) {
            float c1 = 1.4f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private float EaseOutCubic(float x) {
            return 1f - Mathf.Pow(1f - x, 3f);
        }
        #endregion
    }
}