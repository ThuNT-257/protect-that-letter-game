using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI {
    /// <summary>
    /// Handles animated scaling, positioning, and coloring for left/right dialogue avatars.
    /// Active speaker gets a "pop-in" effect; inactive speaker dims and shifts.
    /// </summary>
    public class DialogueAvatarAnimator : MonoBehaviour {
        #region Serialized Fields
        [Header("Left Avatar")]
        [SerializeField] private RectTransform leftAvatarRect;   // Left avatar RectTransform
        [SerializeField] private Image leftAvatarImage;          // Left avatar Image

        [Header("Right Avatar")]
        [SerializeField] private RectTransform rightAvatarRect;  // Right avatar RectTransform
        [SerializeField] private Image rightAvatarImage;         // Right avatar Image

        [Header("Animation Settings")]
        [SerializeField] private float avatarAnimDuration = 0.3f;  // Animation duration
        [SerializeField] private float inactiveScale = 0.75f;      // Scale for inactive avatar
        [SerializeField] private float inactiveShiftX = 50f;       // Horizontal shift for inactive avatar
        [SerializeField] private Color activeAvatarColor = Color.white;                        // Active color
        [SerializeField] private Color inactiveAvatarColor = new Color(0.55f, 0.55f, 0.55f, 1f); // Inactive color
        #endregion

        #region Private Fields
        private Vector2 leftAvatarDefaultPos;   // Cached default position (left)
        private Vector2 rightAvatarDefaultPos;  // Cached default position (right)
        private Coroutine leftAvatarCoroutine;  // Left avatar animation coroutine
        private Coroutine rightAvatarCoroutine; // Right avatar animation coroutine
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Caches the default anchored positions of both avatars
        /// </summary>
        private void Awake() {
            CacheReferencesAndPositions();
        }

        /// <summary>
        /// Stops all running avatar coroutines when disabled
        /// </summary>
        private void OnDisable() {
            SafeStopCoroutine(ref leftAvatarCoroutine);
            SafeStopCoroutine(ref rightAvatarCoroutine);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Animates both avatars based on who's speaking
        /// </summary>
        /// <param name="isLeftSpeaking">True if the left character is speaking</param>
        /// <param name="line">Current dialogue line (contains avatar info)</param>
        /// <param name="isFirstLine">True if this is the very first line (hides inactive avatar)</param>
        public void AnimateAvatars(bool isLeftSpeaking, DialogueLine line, bool isFirstLine) {
            AnimateSingleAvatar(isLeft: true, isSpeaking: isLeftSpeaking, line: line, isFirstLine: isFirstLine);
            AnimateSingleAvatar(isLeft: false, isSpeaking: !isLeftSpeaking, line: line, isFirstLine: isFirstLine);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Caches the initial anchored positions for both avatars
        /// </summary>
        private void CacheReferencesAndPositions() {
            if (leftAvatarRect != null) {
                leftAvatarDefaultPos = leftAvatarRect.anchoredPosition;
            }

            if (rightAvatarRect != null) {
                rightAvatarDefaultPos = rightAvatarRect.anchoredPosition;
            }
        }

        /// <summary>
        /// Safely stops a coroutine and clears the reference
        /// </summary>
        private void SafeStopCoroutine(ref Coroutine coroutine) {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        /// <summary>
        /// Animates a single avatar based on speaking state
        /// </summary>
        private void AnimateSingleAvatar(bool isLeft, bool isSpeaking, DialogueLine line, bool isFirstLine) {
            RectTransform avatarRect = isLeft ? leftAvatarRect : rightAvatarRect;
            Image avatarImg = isLeft ? leftAvatarImage : rightAvatarImage;
            Vector2 defaultPos = isLeft ? leftAvatarDefaultPos : rightAvatarDefaultPos;

            // Validate references
            if (avatarRect == null || avatarImg == null) return;

            // Load new avatar sprite if speaker has one
            if (isSpeaking && line != null && !string.IsNullOrEmpty(line.avatar)) {
                string avatarPath = line.avatar;

                // Strip file extension if present
                if (avatarPath.Contains(".")) {
                    avatarPath = System.IO.Path.GetFileNameWithoutExtension(avatarPath);
                }

                Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{avatarPath}");
                if (avatarSprite != null) {
                    avatarImg.sprite = avatarSprite;
                }
            }

            // Hide avatar if no sprite is assigned
            if (avatarImg.sprite == null) {
                avatarRect.gameObject.SetActive(false);
                return;
            }

            // Hide inactive avatar on the first line (clean intro)
            if (isFirstLine && !isSpeaking) {
                avatarRect.gameObject.SetActive(false);
                return;
            }

            avatarRect.gameObject.SetActive(true);

            // Calculate target transforms based on speaking state
            Vector3 targetScale = isSpeaking ? Vector3.one : new Vector3(inactiveScale, inactiveScale, 1f);
            Vector2 targetPos = defaultPos;
            Color targetColor = isSpeaking ? activeAvatarColor : inactiveAvatarColor;

            if (!isSpeaking) {
                // Shift inactive avatar horizontally
                float shiftX = isLeft ? -inactiveShiftX : inactiveShiftX;
                targetPos.x += shiftX;

                // Compensate vertical position for scaling (keep feet anchored)
                float height = avatarRect.rect.height;
                float scaleDifference = 1f - inactiveScale;
                float bottomOffset = height * scaleDifference * avatarRect.pivot.y;
                targetPos.y -= bottomOffset;
            }

            // Start the animation coroutine for the correct side
            if (isLeft) {
                SafeStopCoroutine(ref leftAvatarCoroutine);
                leftAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(
                    avatarRect, avatarImg, targetScale, targetPos, targetColor, isSpeaking));
            } else {
                SafeStopCoroutine(ref rightAvatarCoroutine);
                rightAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(
                    avatarRect, avatarImg, targetScale, targetPos, targetColor, isSpeaking));
            }
        }

        /// <summary>
        /// Easing function: overshoots slightly past the target ("pop" effect)
        /// </summary>
        private float EaseOutBack(float x) {
            float c1 = 1.4f;
            float c3 = c1 + 4f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        /// <summary>
        /// Easing function: smooth deceleration
        /// </summary>
        private float EaseOutCubic(float x) {
            return 1f - Mathf.Pow(1f - x, 3f);
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Coroutine that animates scale, position, and color over time
        /// </summary>
        private IEnumerator AnimateAvatarRoutine(RectTransform avatarRect, Image img,
            Vector3 targetScale, Vector2 targetPos, Color targetColor, bool isSpeaking) {
            float elapsed = 0f;
            Vector3 startScale = avatarRect.localScale;
            Vector2 startPos = avatarRect.anchoredPosition;
            Color startColor = img != null ? img.color : Color.white;

            while (elapsed < avatarAnimDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / avatarAnimDuration);

                // Use "pop" easing for active, smooth for inactive
                float scaleT = isSpeaking ? EaseOutBack(t) : EaseOutCubic(t);

                // Apply transforms
                avatarRect.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleT);
                avatarRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, EaseOutCubic(t));

                if (img != null) {
                    img.color = Color.Lerp(startColor, targetColor, t);
                }

                yield return null;
            }

            // Ensure exact final state
            avatarRect.localScale = targetScale;
            avatarRect.anchoredPosition = targetPos;
            if (img != null) {
                img.color = targetColor;
            }
        }
        #endregion
    }
}